using CoreDefinitions;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the address");
            var defaultAddress = "http://localhost:62752/ua";
            Console.WriteLine($"Could it be {defaultAddress}?");
            var address = Console.ReadLine();
            if (address.Equals(string.Empty))
            {
                address = defaultAddress;
            }
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
            var solvedInterestingAutomata = new List<ISolvedOptionalAutomaton>();
            long id = 0;
            using (var askingSemaphore = new Semaphore(0, 1))
            {
                connection.On(
                    "ComputeAutomata",
                    async (long localID, int automatonSize, int[] unaryAutomataIndices) =>
                    {
                        id = localID;
                        Console.WriteLine($"Received id: {localID} size: {automatonSize} and many {unaryAutomataIndices.Length} unary automata");
                        foreach (var a in unaryAutomataIndices)
                        {
                            Console.Write($",{a}");
                        }
                        Console.WriteLine();
                        solvedInterestingAutomata.Clear();
                    // do some work...

                    await Task.Delay(3000);
                        Console.WriteLine("done");
                        askingSemaphore.Release();
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
                        askingSemaphore.Release();
                    }
                };
                connection.StartAsync().Wait();
                connection.InvokeAsync("SendUnaryAutomataIndices", GetAutomataCount()).Wait();
                while (connection.State == HubConnectionState.Connected)
                {
                    askingSemaphore.WaitOne();
                    if (connection.State == HubConnectionState.Connected)
                        connection
                            .InvokeAsync(
                                "ReceiveSolvedUnaryAutomatonAndAskForMore",
                                id,
                                solvedInterestingAutomata,
                                GetAutomataCount()
                            )
                            .Wait();
                }
            }
            #endregion
        }

        private static int GetAutomataCount()
        {
            return 32;
        }
    }
}
