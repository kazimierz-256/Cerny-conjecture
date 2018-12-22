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
                var threads = Environment.ProcessorCount;
                var minimum = Environment.ProcessorCount * 4;
                var size = -1;
                var minimalLength = -1;
                var queue = new ConcurrentQueue<int>();
                var recommendedIntake = minimum * 4;
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
                        minimalLength = serverMinimalLength;
                    }
                );

                connection.Closed += async (error) =>
                {
                    if (shouldReconnect)
                    {
                        Console.WriteLine(":( Closed connection. Reconnecting...");

                        await Task.Delay(
                            (int)(Math.Exp(new Random().Next(0, 6)) * 10)
                            );
                        await connection.StartAsync();
                    }
                    else
                    {
                        Console.WriteLine("Connection ended :)");
                    }
                };
                #endregion

                connection.StartAsync().Wait();

                TaskManager<int> taskManager;
                void setupManager()
                {
                    taskManager = new TaskManager<int>(
                      threads,
                      leftover => (leftover <= minimum),
                      index =>
                      {
                          var list = new List<ISolvedOptionalAutomaton>();
                          foreach (var automaton in BinaryAutomataIterator.GetAllWithLongSynchronizedWord(() => minimalLength, size, index))
                              list.Add(automaton.DeepClone());

                          resultsMerged.Enqueue(new Tuple<int, List<ISolvedOptionalAutomaton>>(index, list));
                      },
                      async () =>
                      {
                          lock (cancelSync)
                          {
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
                            recommendedIntake *= 2;
                            minimum *= 2;
                            cancellationToken.Dispose();
                            cancellationToken = new CancellationTokenSource();
                        }
                    }

                    var toSendIndices = new List<int>();
                    var toSendSolved = new List<List<ISolvedOptionalAutomaton>>();
                    while (resultsMerged.TryDequeue(out var item))
                    {
                        toSendIndices.Add(item.Item1);
                        toSendSolved.Add(item.Item2);
                    }

                    connection.InvokeAsync("ReceiveSolvedUnaryAutomatonAndAskForMore", toSendIndices, toSendSolved, recommendedIntake).Wait();
                    Task.Delay(TimeSpan.FromSeconds(20), cancellationToken.Token);
                }
            }
        }
    }
}
