using BinaryAutomataChecking;
using CommunicationContracts;
using CoreDefinitions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private class MicroSolution
        {
            public int unaryIndex;
            public byte[] unaryItself;
            public List<ISolvedOptionalAutomaton> solvedAutomata;

            public MicroSolution(int unaryIndex, byte[] unaryItself, List<ISolvedOptionalAutomaton> solvedAutomata)
            {
                this.unaryIndex = unaryIndex;
                this.unaryItself = unaryItself;
                this.solvedAutomata = solvedAutomata;
            }
        }
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
            while (true)
            {
                try
                {
                    using (
                        Semaphore questSemaphore = new Semaphore(0, int.MaxValue),
                         solutionSemaphore = new Semaphore(0, int.MaxValue)
                         )
                    {
                        var makeSound = false;
                        if (args.Length >= 3)
                        {
                            if (int.TryParse(args[2], out var makeIt))
                                if (makeIt != 0)
                                    makeSound = true;
                        }
                        var maximumAutomatonCollectionSize = int.MaxValue;
                        var targetTimeout = TimeSpan.FromSeconds(10);
                        var threads = args.Length >= 2 ? int.Parse(args[1]) : Environment.ProcessorCount;
                        var minimalIntake = threads;
                        var recommendedIntake = minimalIntake * 4;
                        var askedForMore = true;

                        var size = -1;
                        var minimalSynchronizingLength = 0;
                        var unaryResourcesQueue = new ConcurrentQueue<int>();
                        var resultsMerged = new ConcurrentQueue<MicroSolution>();
                        var resultsMergedTotalAutomata = 0;

                        #region address setup
                        var address = $"http://localhost:62752/ua";
                        if (args.Length >= 1)
                        {
                            address = $"{args[0]}/ua";
                        }
                        Console.WriteLine($"Connecting to {address}");
                        #endregion

                        #region WebSocket connection
                        var shouldReconnect = true;
                        var connection = new HubConnectionBuilder()
                            .WithUrl(address)
                            .AddMessagePackProtocol()
                            .Build();

                        TaskManager<int> taskManager = null;
                        connection.On("NoMoreAutomataThankYou", async () =>
                            {
                                SayColoured(ConsoleColor.Magenta, "No more automata, thanks");
                                shouldReconnect = false;
                                await connection.StopAsync();
                            }
                        );
                        var firstTimeSetup = true;
                        connection.On("ComputeAutomata", async (ServerClientSentUnaryAutomataWithSettings parameters) =>
                            {
                                targetTimeout = TimeSpan.FromSeconds(parameters.targetTimeoutSeconds);
                                maximumAutomatonCollectionSize = parameters.targetCollectionSize;

                                if (parameters.serverMinimalLength != minimalSynchronizingLength)
                                    SayColoured(ConsoleColor.Red, $"New minimal from server {parameters.serverMinimalLength} out of {(parameters.automatonSize - 1) * (parameters.automatonSize - 1)}");

                                // no need for pedantic synchronization, the server will ultimately handle everything synchronously
                                if (minimalSynchronizingLength < parameters.serverMinimalLength)
                                    minimalSynchronizingLength = parameters.serverMinimalLength;

                                if (firstTimeSetup)
                                {
                                    firstTimeSetup = false;
                                    size = parameters.automatonSize;
                                    #region SETUP

                                    taskManager?.Abort();

                                    taskManager = new TaskManager<int>(
                                    threads,
                                    leftover => (leftover <= minimalIntake),
                                    index =>
                                    {
                                        var list = new List<ISolvedOptionalAutomaton>();
                                        var uniqueAutomaton = UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(size, index);
                                        var uniqueAutomatonBytes = new byte[uniqueAutomaton.Length];
                                        for (int i = 0; i < uniqueAutomaton.Length; i++)
                                            uniqueAutomatonBytes[i] = (byte)uniqueAutomaton[i];

                                        foreach (var automaton in new BinaryAutomataIterator().GetAllSolved(size, index))
                                        {
                                            if (automaton.SynchronizingWordLength.HasValue && automaton.SynchronizingWordLength.Value >= minimalSynchronizingLength)
                                                list.Add(automaton.DeepClone());

                                            if (list.Count > 0 && !list[list.Count - 1].SynchronizingWordLength.HasValue)
                                            {
                                                throw new Exception("Not synchronizable!");
                                            }
                                        }

                                        if (list.Count > 0)
                                            SayColoured(ConsoleColor.Blue, $"Completed unary {index} (found {list.Count})");
#if DEBUG
                                        else
                                            SayColoured(ConsoleColor.DarkGray, $"Completed unary {index} (not found any)");
#endif
                                        resultsMerged.Enqueue(new MicroSolution(index, uniqueAutomatonBytes, list));
                                        Interlocked.Add(ref resultsMergedTotalAutomata, list.Count);
                                    },
                                    async () =>
                                    {
                                        solutionSemaphore.Release();
                                    },
                                    unaryResourcesQueue,
                                    questSemaphore
                                    );

                                    taskManager.Launch();
                                    #endregion
                                }
                                else if (size != parameters.automatonSize)
                                {
                                    Console.WriteLine("Received incorrect automaton size.");
                                    return;
                                }
                                SayColoured(ConsoleColor.Green, $"Received {parameters.unaryAutomataIndices.Count} unary automata of size {parameters.automatonSize}");
#if DEBUG
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
                                    unaryResourcesQueue.Enqueue(unaryIndex);
                                    questSemaphore.Release();
                                }
                                askedForMore = false;
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
                                        SayColoured(ConsoleColor.Red, "Closed connection. Reconnecting...");
#endif
                                        await Task.Delay((int)(Math.Exp(new Random().Next(0, 6))));
                                        await connection.StartAsync();
                                    }
                                    else
                                    {
#if DEBUG
                                        SayColoured(ConsoleColor.Magenta, "Connection ended");
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
                        var afterSentDate = DateTime.Now;
                        while (connection.State == HubConnectionState.Connected)
                        {
                            var previousMinimalSyncLength = minimalSynchronizingLength;
                            var results = PrepareNextRound(resultsMerged, ref resultsMergedTotalAutomata, ref maximumAutomatonCollectionSize, ref minimalSynchronizingLength, out var toSendAutomataCount);
                            if (minimalSynchronizingLength != previousMinimalSyncLength)
                                SayColoured(ConsoleColor.Red, $"Self-changed the minimal sync length from {previousMinimalSyncLength} to {minimalSynchronizingLength}");

                            // might still throw an exception, we allow that
                            if (connection.State == HubConnectionState.Connected)
                            {
                                if (firstTime || results.Count > 0)
                                {
                                    firstTime = false;
                                    var parameters = new ClientServerRequestForMoreAutomata()
                                    {
                                        nextQuantity = recommendedIntake - unaryResourcesQueue.Count,
                                        suggestedMinimumBound = minimalSynchronizingLength,
                                        solutions = results
                                    };
                                    var command = connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", parameters);
                                    if (makeSound)
                                        Console.Beep();
                                    command.Wait();

                                    #region Sent back description
                                    Console.Write("Sent back ");
                                    SayColoured(ConsoleColor.DarkGreen, parameters.solutions.Count.ToString(), false);
                                    Console.Write(" unary solutions consisting of ");
                                    SayColoured(ConsoleColor.DarkGreen, toSendAutomataCount.ToString(), false);
                                    Console.WriteLine(" automata");

                                    solvedUnaryAutomata += results.Count;
                                    var speed = solvedUnaryAutomata / (DateTime.Now - solveBeginningTime).TotalSeconds;
                                    SayColoured(ConsoleColor.DarkGray, $"Total speed: {speed:F2} unary automata per second");
                                    #endregion

                                    afterSentDate = DateTime.Now;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Bye!");
                                return;
                            }

                            var elapsedTimeout = TimeSpan.Zero;
                            do
                            {
                                solutionSemaphore.WaitOne(targetTimeout - elapsedTimeout);
                                elapsedTimeout = DateTime.Now - afterSentDate;
                                if (!askedForMore && unaryResourcesQueue.Count <= minimalIntake)
                                    break;
                            } while (targetTimeout > elapsedTimeout);

                            if (!askedForMore && unaryResourcesQueue.Count <= minimalIntake)
                            {
                                askedForMore = true;
                                minimalIntake += Environment.ProcessorCount;
                                recommendedIntake += Environment.ProcessorCount * 4;
                                SayColoured(ConsoleColor.DarkRed, $"Increased recommended intake to {recommendedIntake}");
                            }


                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Reconnecting...");
                }
            }

        }

        static List<SolvedAutomatonMessage> PrepareNextRound(
            ConcurrentQueue<MicroSolution> resultsMerged,
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
                Interlocked.Add(ref resultsMergedTotalAutomata, -mergedResult.solvedAutomata.Count);

                var syncLengths = new List<ushort>();
                var solvedBs = new List<byte[]>();
                foreach (var automaton in mergedResult.solvedAutomata)
                {
                    syncLengths.Add(automaton.SynchronizingWordLength.Value);
                    solvedBs.Add(automaton.TransitionFunctionsB);

                    if (!synchronizingLengthToCount.ContainsKey(automaton.SynchronizingWordLength.Value))
                        synchronizingLengthToCount.Add(automaton.SynchronizingWordLength.Value, 0);
                    synchronizingLengthToCount[automaton.SynchronizingWordLength.Value] += 1;
                }

                results.Add(new SolvedAutomatonMessage()
                {
                    unaryIndex = mergedResult.unaryIndex,
                    solvedB = solvedBs,
                    solvedSyncLength = syncLengths,
                    unaryArray = mergedResult.unaryItself
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
