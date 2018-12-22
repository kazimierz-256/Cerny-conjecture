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
        static void Main(string[] args)
        {
            using (var semaphore = new Semaphore(0, int.MaxValue))
            {
                var targetTimeout = TimeSpan.FromSeconds(20);
                var threads = 1;// Environment.ProcessorCount;
                var minimum = Environment.ProcessorCount;
                var recommendedIntake = minimum * 4;

                var size = -1;
                var minimalLength = -1;
                var queue = new ConcurrentQueue<int>();
                var resultsMerged = new ConcurrentQueue<Tuple<int, List<ISolvedOptionalAutomaton>>>();
                var cancellationToken = new CancellationTokenSource();
                var cancelSync = new object();

                #region address setup
                Console.WriteLine("Please enter the address");
                var defaultAddress = "http://localhost:62752/ua";
                Console.WriteLine($"Could it be {defaultAddress}?");
                var address = Console.ReadLine();

                if (address.Equals(string.Empty))
                    address = defaultAddress;
                #endregion

                #region WebSocket connection
                var shouldReconnect = true;
                var connection = new HubConnectionBuilder()
                    .WithUrl(address)
                    .Build();

                connection.On(
                    "NoMoreAutomataThankYou",
                    async () =>
                    {
#if DEBUG
                        Console.WriteLine("No more automata, thanks");
#endif
                        shouldReconnect = false;
                        await connection.StopAsync();
                    }
                    );
                connection.On(
                    "ComputeAutomata",
                    (int automatonSize, int serverMinimalLength, List<int> unaryAutomataIndices) =>
                    {
                        minimalLength = serverMinimalLength;
                        if (size == -1)
                        {
                            size = automatonSize;
                            setupManager();
                        }
#if DEBUG
                        Console.WriteLine($"Received size: {automatonSize} and many {unaryAutomataIndices.Count} unary automata");
                        foreach (var a in unaryAutomataIndices)
                        {
                            Console.Write($",{a}");
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
                    (int serverMinimalLength) =>
                    {
#if DEBUG
                        Console.WriteLine("Updated length!");
#endif
                        minimalLength = serverMinimalLength;
                    }
                );

                connection.Closed += async (error) =>
                {
                    if (shouldReconnect)
                    {
#if DEBUG
                        Console.WriteLine(":( Closed connection. Reconnecting...");
#endif
                        await Task.Delay(
                            (int)(Math.Exp(new Random().Next(0, 6)) * 10)
                            );
                        await connection.StartAsync();
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine("Connection ended :)");
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

                TaskManager<int> taskManager;
                void setupManager()
                {
                    taskManager = new TaskManager<int>(
                      threads,
                      leftover => (leftover <= minimum),
                      index =>
                      {
#if DEBUG
                          Console.WriteLine($"Ha! Completed unary {index}");
#endif
                          var list = new List<ISolvedOptionalAutomaton>();
                          foreach (var automaton in new BinaryAutomataIterator().GetAllSolved(size, index))
                          {
                              if (automaton.SynchronizingWordLength.HasValue && automaton.SynchronizingWordLength.Value >= minimalLength)
                                  list.Add(automaton.DeepClone());
                          }

                          resultsMerged.Enqueue(new Tuple<int, List<ISolvedOptionalAutomaton>>(index, list));
                      },
                      async () =>
                      {
                          lock (cancelSync)
                          {
#if DEBUG
                              Console.WriteLine("Running out of resources! Scarce number! Please give more!");
#endif
                              if (!cancellationToken.IsCancellationRequested)
                                  cancellationToken.Cancel();
                          }
                      },
                      queue,
                      semaphore
                      );

                    taskManager.Launch();
                }

                while (connection.State == HubConnectionState.Connected)
                {
                    lock (cancelSync)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
#if DEBUG
                            Console.WriteLine($"Canceled!");
#endif
                            recommendedIntake *= 2;
                            minimum *= 2;
                            var oldToken = cancellationToken;
                            cancellationToken = new CancellationTokenSource();
                            oldToken.Dispose();
                        }
                        else
                        {
#if DEBUG
                            Console.WriteLine($"Timed out, good timing :)");
#endif
                        }
                    }

                    var toSendIndices = new List<int>();
                    var toSendSolved = new List<List<ISolvedOptionalAutomaton>>();
                    while (resultsMerged.TryDequeue(out var item))
                    {
                        toSendIndices.Add(item.Item1);
                        toSendSolved.Add(item.Item2);
                    }

                    // might still throw an exception, we allow that
                    if (connection.State == HubConnectionState.Connected)
                    {
                        //try
                        {
                            connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", toSendIndices, toSendSolved, recommendedIntake).Wait();
                        }
                        //catch (Exception e)
                        //{
                        //    Console.WriteLine("Inappropriate call, please don't mind. The connection is ended either way.");
                        //}
                    }
                    Task.Delay(targetTimeout, cancellationToken.Token);
#if DEBUG
                    Console.WriteLine($"Woken up");
#endif
                }
            }
        }
    }
}
