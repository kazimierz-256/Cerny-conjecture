using BinaryAutomataChecking;
using CommunicationContracts;
using CoreDefinitions;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static object consoleSync = new object();
        private static void SayColoured(ConsoleColor color, string text, bool newline = true)
        {
            lock (consoleSync)
            {
                Console.ForegroundColor = color;

                if (newline)
                    Console.WriteLine(text);
                else
                    Console.Write(text);

                Console.ResetColor();
            }
        }
        static void Main(string[] args)
        {
            using (var semaphore = new Semaphore(0, int.MaxValue))
            {
                var maximumAutomatonCollectionSize = int.MaxValue;
                var targetTimeout = TimeSpan.FromSeconds(30);
                var artificialWakeup = false;
                var threads = Environment.ProcessorCount;
                var minimalIntake = Environment.ProcessorCount;
                var recommendedIntake = minimalIntake * 4;

                var size = -1;
                var minimalSynchronizingLength = 0;
                var queue = new ConcurrentQueue<int>();
                var resultsMerged = new ConcurrentQueue<Tuple<int, List<ISolvedOptionalAutomaton>>>();
                var resultsMergedTotalAutomata = 0;
                var cancellationToken = new CancellationTokenSource();
                var cancelSync = new object();

                #region address setup
                Console.WriteLine("Hello User :) XO XO Please enter the address");
                var defaultAddress = "http://localhost:62752/ua";
                Console.WriteLine($"Could it be {defaultAddress}? If yes, just press enter.");
                var address = Console.ReadLine();

                if (address.Equals(string.Empty))
                    address = defaultAddress;
                #endregion

                #region WebSocket connection
                var shouldReconnect = true;
                var connection = new HubConnectionBuilder()
                    .WithUrl(address)
                    .Build();

                TaskManager<int> taskManager = null;
                connection.On("NoMoreAutomataThankYou", async () =>
                    {
                        SayColoured(ConsoleColor.Magenta, "No more automata, thanks");
                        shouldReconnect = false;
                        await connection.StopAsync();
                    }
                );
                connection.On("ComputeAutomata", async (ServerClientSentUnaryAutomataWithSettings parameters) =>
                    {
                        maximumAutomatonCollectionSize = parameters.targetCollectionSize;

                        if (parameters.serverMinimalLength != minimalSynchronizingLength)
                            SayColoured(ConsoleColor.Red, $"New minimal from server {parameters.serverMinimalLength} out of {(parameters.automatonSize - 1) * (parameters.automatonSize - 1)}");

                        if (minimalSynchronizingLength < parameters.serverMinimalLength)
                            minimalSynchronizingLength = parameters.serverMinimalLength;

                        if (size == -1)
                        {
                            size = parameters.automatonSize;
                            #region SETUP

                            taskManager?.Abort();

                            taskManager = new TaskManager<int>(
                            threads,
                            leftover => (leftover <= minimalIntake),
                            index =>
                            {
                                var list = new List<ISolvedOptionalAutomaton>();
                                foreach (var automaton in new BinaryAutomataIterator().GetAllSolved(size, index))
                                {
                                    if (automaton.SynchronizingWordLength.HasValue && automaton.SynchronizingWordLength.Value >= minimalSynchronizingLength)
                                        list.Add(automaton.DeepClone());

                                    if (list.Count > 0 && !list[list.Count - 1].SynchronizingWordLength.HasValue)
                                    {
                                        throw new Exception("Not synchronizable!");
                                    }
                                }
#if DEBUG
                                if (list.Count == 0)
                                    SayColoured(ConsoleColor.DarkGray, $"Completed unary {index} (not found any)");
                                else
                                    SayColoured(ConsoleColor.Blue, $"Completed unary {index} (found {list.Count})");
#endif
                                resultsMerged.Enqueue(new Tuple<int, List<ISolvedOptionalAutomaton>>(index, list));
                                Interlocked.Add(ref resultsMergedTotalAutomata, list.Count);

                                if (resultsMergedTotalAutomata >= maximumAutomatonCollectionSize && !cancellationToken.IsCancellationRequested)
                                {
#if DEBUG
                                    SayColoured(ConsoleColor.Red, "Critical automaton count reached!");
#endif
                                    lock (cancelSync)
                                    {
                                        artificialWakeup = true;
                                        if (!cancellationToken.IsCancellationRequested)
                                            cancellationToken.Cancel();
                                    }
                                }
                            },
                            async () =>
                            {
                                lock (cancelSync)
                                {
#if DEBUG
                                    SayColoured(ConsoleColor.Green, "Running out of resources! Scarce number! Please give more!");
#endif
                                    if (!cancellationToken.IsCancellationRequested)
                                        cancellationToken.Cancel();
                                }
                            },
                            queue,
                            semaphore
                            );

                            taskManager.Launch();
                            #endregion
                        }
                        else if (size != parameters.automatonSize)
                        {
                            Console.WriteLine("Received incorrect automaton size.");
                            return;
                        }
#if DEBUG
                        SayColoured(ConsoleColor.Green, $"Received {parameters.unaryAutomataIndices.Count} unary automata of size {parameters.automatonSize}");
                        var first = true;
                        foreach (var a in parameters.unaryAutomataIndices)
                        {
                            if (first)
                                Console.Write($"{a}");
                            else
                                Console.Write($",{a}");

                            first = false;
                        }
                        Console.WriteLine();
#endif

                        foreach (var unaryIndex in parameters.unaryAutomataIndices)
                        {
                            queue.Enqueue(unaryIndex);
                            semaphore.Release();
                        }

                    }
                );

                #region Updating minimum synchronizing word length
                connection.On("UpdateLength", async (int serverMinimalLength) =>
                            {
                                SayColoured(ConsoleColor.DarkRed, $"Dynamically updated length to {serverMinimalLength}!");
                                minimalSynchronizingLength = serverMinimalLength;
                            }
                        );
                #endregion

                #region on connection close
                connection.Closed += async (error) =>
                        {
                            if (shouldReconnect)
                            {
#if DEBUG
                                SayColoured(ConsoleColor.Red, ":( Closed connection. Reconnecting...");
#endif
                                await Task.Delay((int)(Math.Exp(new Random().Next(0, 6)) * 10));
                                await connection.StartAsync();
                            }
                            else
                            {
#if DEBUG
                                SayColoured(ConsoleColor.Magenta, "Connection ended :)");
#endif
                            }
                        };
                #endregion
                #endregion
                var repeat = true;
                while (repeat)
                {
                    repeat = false;
                    try
                    {
                        connection.StartAsync().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not connect. Retrying...");
                        repeat = true;
                    }
                }

                var firstTime = true;
                var solvedUnaryAutomata = 0;
                var solveBeginningTime = DateTime.Now;
                while (connection.State == HubConnectionState.Connected)
                {
                    lock (cancelSync)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
#if DEBUG
                            SayColoured(ConsoleColor.Yellow, $"Canceled!");
#endif
                            var oldToken = cancellationToken;
                            cancellationToken = new CancellationTokenSource();
                            oldToken.Dispose();
                        }
                        else
                        {
#if DEBUG
                            SayColoured(ConsoleColor.Magenta, $"Timed out, good timing :)");
#endif
                        }
                    }

                    var results = PrepareNextRound(resultsMerged, ref resultsMergedTotalAutomata, ref maximumAutomatonCollectionSize, ref minimalSynchronizingLength, out var toSendAutomataCount);

                    // might still throw an exception, we allow that
                    if (connection.State == HubConnectionState.Connected)
                    {
                        if (firstTime || results.Count > 0)
                        {
                            firstTime = false;
                            var parameters = new ClientServerRequestForMoreAutomata()
                            {
                                nextQuantity = recommendedIntake,
                                suggestedMinimumBound = minimalSynchronizingLength,
                                solutions = results
                            };
                            connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", parameters).Wait();

                            Console.Write("Sent back ");
                            SayColoured(ConsoleColor.DarkGreen, parameters.solutions.Count.ToString(), false);
                            Console.Write(" unary solutions consisting of ");
                            SayColoured(ConsoleColor.DarkGreen, toSendAutomataCount.ToString(), false);
                            Console.WriteLine(" automata");

                            solvedUnaryAutomata += results.Count;
                            var speed = solvedUnaryAutomata / (DateTime.Now - solveBeginningTime).TotalSeconds;
                            SayColoured(ConsoleColor.DarkGray, $"Total speed: {speed:F2} unary automata per second");

                        }
                    }
                    else
                    {
                        Console.WriteLine("Bye!");
                        return;
                    }
                    var beforeSleep = DateTime.Now;
                    cancellationToken.Token.WaitHandle.WaitOne();
                    if ((DateTime.Now - beforeSleep) < targetTimeout && !artificialWakeup)
                    {
#if DEBUG
                        SayColoured(ConsoleColor.Green, "Increased recommended intake");
#endif
                        recommendedIntake *= 2;
                        minimalIntake *= 2;
                    }
#if DEBUG
                    SayColoured(ConsoleColor.Green, $"Woken up" + (artificialWakeup ? " artificially" : string.Empty));
#endif
                    artificialWakeup = false;
                }
            }

        }

        static List<SolvedAutomatonMessage> PrepareNextRound(
            ConcurrentQueue<Tuple<int, List<ISolvedOptionalAutomaton>>> resultsMerged,
            ref int resultsMergedTotalAutomata,
            ref int maximumAutomatonCollectionSize,
            ref int minimalSynchronizingLength,
            out int toSendAutomataCount
            )
        {
            var results = new List<SolvedAutomatonMessage>();
            var synchronizingLengthToCount = new SortedDictionary<int, int>();

            toSendAutomataCount = 0;
            while (resultsMerged.TryDequeue(out var mergedResult))
            {
                Interlocked.Add(ref resultsMergedTotalAutomata, -mergedResult.Item2.Count);

                var syncLengths = new List<ushort>();
                var solvedBs = new List<byte[]>();
                foreach (var automaton in mergedResult.Item2)
                {
                    syncLengths.Add(automaton.SynchronizingWordLength.Value);
                    solvedBs.Add(automaton.TransitionFunctionsB);

                    if (!synchronizingLengthToCount.ContainsKey(automaton.SynchronizingWordLength.Value))
                        synchronizingLengthToCount.Add(automaton.SynchronizingWordLength.Value, 0);
                    synchronizingLengthToCount[automaton.SynchronizingWordLength.Value] += 1;
                }

                results.Add(new SolvedAutomatonMessage()
                {
                    unaryIndex = mergedResult.Item1,
                    solvedB = solvedBs,
                    solvedSyncLength = syncLengths,
                    // IMPORTANT: assuming first transition function is the same
                    unaryArray = (mergedResult.Item2.Count > 0) ? mergedResult.Item2[0].TransitionFunctionsA : new byte[0]
                });

                toSendAutomataCount += syncLengths.Count;
            }

            if (toSendAutomataCount > maximumAutomatonCollectionSize)
            {
                var leftover = toSendAutomataCount;
                var removeUpTo = -1;
                foreach (var item in synchronizingLengthToCount)
                {
                    if (leftover > maximumAutomatonCollectionSize)
                    {
                        leftover -= item.Value;
                        removeUpTo = item.Key;
                    }
                    else
                    {
                        break;
                    }
                }

                if (removeUpTo + 1 > minimalSynchronizingLength)
                {
                    minimalSynchronizingLength = removeUpTo + 1;
                }
                toSendAutomataCount = leftover;

                foreach (var automatonSolution in results)
                {
                    var newSyncLengths = new List<ushort>();
                    var newSolvedB = new List<byte[]>();
                    for (int i = 0; i < automatonSolution.solvedB.Count; i++)
                    {
                        if (automatonSolution.solvedSyncLength[i] >= minimalSynchronizingLength)
                        {
                            newSyncLengths.Add(automatonSolution.solvedSyncLength[i]);
                            newSolvedB.Add(automatonSolution.solvedB[i]);
                        }
                    }
                    automatonSolution.solvedB = newSolvedB;
                    automatonSolution.solvedSyncLength = newSyncLengths;
                }
            }

            return results;
        }

    }
}
