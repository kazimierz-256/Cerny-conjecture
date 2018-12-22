using BinaryAutomataChecking;
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
        private const int criticalAutomatonCount = 200;
        private static object consoleSync = new object();
        private static void SayColoured(ConsoleColor color, string text)
        {
            lock (consoleSync)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
        static void Main(string[] args)
        {
            using (var semaphore = new Semaphore(0, int.MaxValue))
            {
                var targetTimeout = TimeSpan.FromSeconds(20);
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
                connection.On(
                    "NoMoreAutomataThankYou",
                    async () =>
                    {
#if DEBUG
                        SayColoured(ConsoleColor.Magenta, "No more automata, thanks");
#endif
                        shouldReconnect = false;
                        await connection.StopAsync();
                    }
                    );
                connection.On(
                    "ComputeAutomata",
                    async (int automatonSize, int serverMinimalLength, List<int> unaryAutomataIndices) =>
                    {
#if DEBUG
                        if (serverMinimalLength != minimalSynchronizingLength)
                        {
                            SayColoured(ConsoleColor.Red, $"New minimal {serverMinimalLength} out of {(automatonSize - 1) * (automatonSize - 1)}");
                        }
#endif
                        minimalSynchronizingLength = serverMinimalLength;
                        if (size == -1)
                        {
                            size = automatonSize;
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

                                  if (resultsMergedTotalAutomata >= criticalAutomatonCount && !cancellationToken.IsCancellationRequested)
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
                        else if (size != automatonSize)
                        {
                            Console.WriteLine("Received incorrect automaton size.");
                            return;
                        }
#if DEBUG
                        SayColoured(ConsoleColor.Green, $"Received {unaryAutomataIndices.Count} unary automata of size {automatonSize}");
                        var first = true;
                        foreach (var a in unaryAutomataIndices)
                        {
                            if (first)
                                Console.Write($"{a}");
                            else
                                Console.Write($",{a}");

                            first = false;
                        }
                        Console.WriteLine();
#endif

                        foreach (var unaryIndex in unaryAutomataIndices)
                        {
                            queue.Enqueue(unaryIndex);
                            semaphore.Release();
                        }

                    }
                );

                connection.On(
                    "UpdateLength",
                    async (int serverMinimalLength) =>
                    {
#if DEBUG
                        SayColoured(ConsoleColor.DarkRed, "Updated length!");
#endif
                        minimalSynchronizingLength = serverMinimalLength;
                    }
                );

                connection.Closed += async (error) =>
                {
                    if (shouldReconnect)
                    {
#if DEBUG
                        SayColoured(ConsoleColor.Red, ":( Closed connection. Reconnecting...");
#endif
                        await Task.Delay(
                            (int)(Math.Exp(new Random().Next(0, 6)) * 10)
                            );
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

                    var toSendIndices = new List<int>();
                    var toSendSolvedUnary = new List<byte[]>();
                    var toSendSolvedSyncLength = new List<List<ushort>>();
                    var toSendSolvedB = new List<List<byte[]>>();
                    var toSendAutomataCount = 0;
                    while (resultsMerged.TryDequeue(out var mergedResult))
                    {
                        Interlocked.Add(ref resultsMergedTotalAutomata, -mergedResult.Item2.Count);
                        toSendIndices.Add(mergedResult.Item1);
                        if (mergedResult.Item2.Count > 0)
                        {
                            // IMPORTANT: assuming first transition function is the same
                            toSendSolvedUnary.Add(mergedResult.Item2[0].TransitionFunctionsA);
                        }
                        else
                        {
                            toSendSolvedUnary.Add(new byte[0]);
                        }
                        var syncLengths = new List<ushort>();
                        var solvedBs = new List<byte[]>();
                        foreach (var automaton in mergedResult.Item2)
                        {
                            syncLengths.Add(automaton.SynchronizingWordLength.Value);
                            solvedBs.Add(automaton.TransitionFunctionsB);
                        }

                        toSendSolvedSyncLength.Add(syncLengths);
                        toSendSolvedB.Add(solvedBs);
                        toSendAutomataCount += syncLengths.Count;
                    }

                    // might still throw an exception, we allow that
                    if (connection.State == HubConnectionState.Connected)
                    {
                        if (firstTime || toSendIndices.Count > 0)
                        {
                            firstTime = false;
                            connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", toSendIndices, toSendSolvedUnary, toSendSolvedSyncLength, toSendSolvedB, recommendedIntake).Wait();
#if DEBUG
                            SayColoured(ConsoleColor.DarkRed, $"Sent {toSendIndices.Count} unary solutions consisting of {toSendAutomataCount} automata");
#endif
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
    }
}
