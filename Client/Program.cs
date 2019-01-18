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
            public DateTime startTime;
            public DateTime finishTime;
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
                    DoWork(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Reconnecting...");
                }
                Task.Delay((int)(Math.Exp(new Random().NextDouble() * 2 + 5))).Wait();
            }

        }

        private static void DoWork(string[] args)
        {
            using (
                Semaphore questSemaphore = new Semaphore(0, int.MaxValue),
                 solutionSemaphore = new Semaphore(0, int.MaxValue)
                 )
            {
                var makeSound = false;
                if (args.Length >= 2)
                {
                    if (int.TryParse(args[1], out var makeIt))
                        if (makeIt != 0)
                            makeSound = true;
                }
                var maximumAutomatonCollectionSize = int.MaxValue;

                var minimalSynchronizingLength = 0;
                var unaryResourcesQueue = new ConcurrentQueue<int>();
                MicroSolution results = null;
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

                connection.On("NoMoreAutomataThankYou", async () =>
                {
                    try
                    {
                        SayColoured(ConsoleColor.Magenta, "No more automata needed to compute");
                        shouldReconnect = false;
                        await connection.StopAsync();
                    }
                    catch { }
                }
                );

                var theory = new long[]
                {
                    1, 3, 7, 19, 47, 130, 343, 951, 2615, 7318, 20491, 57903, 163898, 466199, 1328993, 3799624, 10884049, 31241170
                };
                connection.On("ComputeAutomata", async (ServerClientSentUnaryAutomataWithSettings parameters) =>
                            {
                                //try
                                //{

                                maximumAutomatonCollectionSize = parameters.targetCollectionSize;

                                if (parameters.serverMinimalLength != minimalSynchronizingLength)
                                    SayColoured(ConsoleColor.Red, $"New minimal from server {parameters.serverMinimalLength} out of {(parameters.automatonSize - 1) * (parameters.automatonSize - 1)}");

                                // no need for pedantic synchronization, the server will ultimately handle everything synchronously
                                if (minimalSynchronizingLength < parameters.serverMinimalLength)
                                    minimalSynchronizingLength = parameters.serverMinimalLength;


                                try
                                {
                                    var startTime = DateTime.Now;
                                    var list = new List<ISolvedOptionalAutomaton>();
                                    var uniqueAutomaton = UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(parameters.automatonSize, parameters.unaryAutomatonIndex);
                                    var uniqueAutomatonBytes = new byte[uniqueAutomaton.Length];
                                    for (int i = 0; i < uniqueAutomaton.Length; i++)
                                        uniqueAutomatonBytes[i] = (byte)uniqueAutomaton[i];

                                    foreach (var automaton in new BinaryAutomataIterator().GetAllSolved(parameters.automatonSize, parameters.unaryAutomatonIndex))
                                    {
                                        if (automaton.SynchronizingWordLength.HasValue && automaton.SynchronizingWordLength.Value >= minimalSynchronizingLength)
                                            list.Add(automaton.DeepClone());
                                    }

                                    if (list.Count > 0)
                                        SayColoured(ConsoleColor.Blue, $"Completed unary {parameters.unaryAutomatonIndex} (found {list.Count})");
#if DEBUG
                                    else
                                        SayColoured(ConsoleColor.DarkGray, $"Completed unary {parameters.unaryAutomatonIndex} (not found any)");
#endif
                                    var finishedTime = DateTime.Now;
                                    results = new MicroSolution()
                                    {
                                        unaryIndex = parameters.unaryAutomatonIndex,
                                        unaryItself = uniqueAutomatonBytes,
                                        solvedAutomata = list,
                                        startTime = startTime,
                                        finishTime = finishedTime
                                    };
                                    Interlocked.Add(ref resultsMergedTotalAutomata, list.Count);
                                    solutionSemaphore.Release();
                                }
                                catch (Exception)
                                {
                                }

                                SayColoured(ConsoleColor.Green, $"Received a unary automata of size {parameters.automatonSize}");
#if DEBUG
                                Console.Write($"{parameters.unaryAutomatonIndex}");
                                Console.WriteLine();
#endif


                                //catch (Exception)
                                //{
                                //}
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
                        try
                        {
                            await Task.Delay((int)(Math.Exp(new Random().NextDouble() * 6)));
                            await connection.StartAsync();
                        }
                        catch (Exception)
                        {
                        }
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

                var solvedUnaryAutomata = 0;
                var solveBeginningTime = DateTime.Now;
                var afterSentDate = DateTime.Now;

                // TODO: check that server handles this
                connection.InvokeAsync("InitializeConnection").Wait();
                while (connection.State == HubConnectionState.Connected && shouldReconnect)
                {
                    solutionSemaphore.WaitOne();

                    var previousMinimalSyncLength = minimalSynchronizingLength;
                    var preparedResult = PrepareNextRound(results, ref resultsMergedTotalAutomata, ref maximumAutomatonCollectionSize, ref minimalSynchronizingLength, out var toSendAutomataCount);
                    if (minimalSynchronizingLength != previousMinimalSyncLength)
                        SayColoured(ConsoleColor.Red, $"Self-changed the minimal sync length from {previousMinimalSyncLength} to {minimalSynchronizingLength}");

                    // might still throw an exception, we allow that
                    if (connection.State == HubConnectionState.Connected && shouldReconnect)
                    {

                        solvedUnaryAutomata += 1;
                        var automataPerSecond = solvedUnaryAutomata / (DateTime.Now - solveBeginningTime).TotalSeconds;
                        var command = connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", new ClientServerRequestForMoreAutomata()
                        {
                            nextQuantity = 1,
                            suggestedMinimumBound = minimalSynchronizingLength,
                            solution = preparedResult
                        });
                        if (makeSound)
                            Console.Beep();
                        command.Wait();

                        #region Sent back description
                        Console.Write("Sent back a unary solution consisting of ");
                        SayColoured(ConsoleColor.DarkGreen, toSendAutomataCount.ToString(), false);
                        Console.WriteLine(" automata");

                        SayColoured(ConsoleColor.DarkGray, $"Total speed including communication time gaps: {automataPerSecond:F2} unary automata per second");
                        #endregion

                        afterSentDate = DateTime.Now;
                    }
                    else
                    {
                        Console.WriteLine("Bye!");
                        return;
                    }

                    var elapsedTimeout = TimeSpan.Zero;
                    if (!shouldReconnect)
                        return;
                }
            }
        }

        private static int IgnoreThreshold(int n) => (n - 1) * (n - 1);

        private static SolvedAutomatonMessage PrepareNextRound(
            MicroSolution solution,
            ref int resultsMergedTotalAutomata,
            ref int maximumAutomatonCollectionSize,
            ref int minimalSynchronizingLength,
            out int toSendAutomataCount
            )
        {
            SolvedAutomatonMessage result = null;
            var synchronizingLengthToCount = new SortedDictionary<int, int>();

            toSendAutomataCount = 0;
            Interlocked.Add(ref resultsMergedTotalAutomata, -solution.solvedAutomata.Count);

            var syncLengths = new List<ushort>();
            var solvedBs = new List<byte[]>();
            var ignoreLimitsThreshold = IgnoreThreshold(solution.unaryItself.Length);

            foreach (var automaton in solution.solvedAutomata)
            {
                syncLengths.Add(automaton.SynchronizingWordLength.Value);
                if (automaton.SynchronizingWordLength.Value < ignoreLimitsThreshold)
                {
                    toSendAutomataCount += 1;
                }
                solvedBs.Add(automaton.TransitionFunctionsB);

                if (!synchronizingLengthToCount.ContainsKey(automaton.SynchronizingWordLength.Value))
                    synchronizingLengthToCount.Add(automaton.SynchronizingWordLength.Value, 0);
                synchronizingLengthToCount[automaton.SynchronizingWordLength.Value] += 1;
            }

            result = new SolvedAutomatonMessage()
            {
                unaryIndex = solution.unaryIndex,
                solvedB = solvedBs,
                solvedSyncLength = syncLengths,
                unaryArray = solution.unaryItself,
                startTime = solution.startTime,
                finishTime = solution.finishTime
            };

            if (toSendAutomataCount > maximumAutomatonCollectionSize)
            {
                var removeUpTo = -1;
                foreach (var item in synchronizingLengthToCount)
                {
                    if (toSendAutomataCount > maximumAutomatonCollectionSize)
                    {
                        toSendAutomataCount -= item.Value;
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

                var newSyncLengths = new List<ushort>();
                var newSolvedB = new List<byte[]>();
                for (int i = 0; i < result.solvedB.Count; i++)
                {
                    if (result.solvedSyncLength[i] >= minimalSynchronizingLength)
                    {
                        newSyncLengths.Add(result.solvedSyncLength[i]);
                        newSolvedB.Add(result.solvedB[i]);
                    }
                }
                result.solvedB = newSolvedB;
                result.solvedSyncLength = newSyncLengths;
            }

            return result;
        }

    }
}
