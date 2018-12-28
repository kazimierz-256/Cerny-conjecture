﻿using CommunicationContracts;
using CoreDefinitions;
using CoreServer.UnaryAutomataDatabase;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.Hubs
{
    public class UnaryAutomataHub : Hub
    {
        private readonly UnaryAutomataDB database;

        public UnaryAutomataHub(UnaryAutomataDB database) => this.database = database;

        private const int targetTimeout = 30;
        private const string solversGroup = "solvers";
        private DateTime lastTimeSaved = DateTime.MinValue;
        private TimeSpan saveMinimumInterval = TimeSpan.FromMinutes(1);
        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(ClientServerRequestForMoreAutomata parameters)
        {
            if (parameters.solutions.Count == 0)
                await Groups.AddToGroupAsync(Context.ConnectionId, solversGroup);
            else
            {
                database.ProcessInterestingAutomata(parameters, out var changedMinimum);

                if (changedMinimum)
                    await Clients.Group(solversGroup).SendAsync("UpdateLength", database.MinimalLength);
            }
            if (parameters.nextQuantity > 0)
            {
                // no need to limit the quantity (in case of overflow automata are being recomputed)
                var automataIndices = database.GetUnaryAutomataToProcessAndMarkAsProcessing(parameters.nextQuantity);
                if (automataIndices.Count > 0)
                {
                    var parameter = new ServerClientSentUnaryAutomataWithSettings()
                    {
                        automatonSize = database.Size,
                        serverMinimalLength = database.MinimalLength,
                        unaryAutomataIndices = automataIndices,
                        targetCollectionSize = UnaryAutomataDB.MaximumLongestAutomataCount,
                        targetTimeoutSeconds = targetTimeout
                    };
                    await Clients.Caller.SendAsync("ComputeAutomata", parameter);
                }
                else
                {
                    await Clients.Group(solversGroup).SendAsync("NoMoreAutomataThankYou");
                }
            }

            if (parameters.solutions.Count > 0)
            {
                await SendStatisticsToAll();
                if (DateTime.Now - lastTimeSaved > saveMinimumInterval)
                {
                    lastTimeSaved = DateTime.Now;
                    await ProgressIO.ProgressIO.ExportStateAsync(database);
                }
            }
        }

        ///// Statistics
        private const string statisticsGroup = "statistics";
        public async Task SendStatistics() => await Clients.Caller.SendAsync("ShowSimpleTextStatistics", database.DumpStatistics());
        public async Task SendStatisticsToAll() => await Clients.Group(statisticsGroup).SendAsync("ShowSimpleTextStatistics", database.DumpStatistics());
        public async Task SubscribeAndSendStatistics()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, statisticsGroup);
            await SendStatistics();
        }
    }
}