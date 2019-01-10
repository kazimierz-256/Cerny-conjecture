using CommunicationContracts;
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
        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(ClientServerRequestForMoreAutomata parameters)
        {
            if (parameters.solutions.Count == 0)
                await Groups.AddToGroupAsync(Context.ConnectionId, solversGroup);
            else
            {
                // different answer size!
                if (parameters.solutions[0].unaryArray.Length != database.Size)
                    return;

                database.RecordSpeed(parameters.automataPerSecond, Context.ConnectionId);
                database.ProcessInterestingAutomata(parameters, out var changedMinimum, Context.ConnectionId);

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
                        targetCollectionSize = database.MaximumLongestAutomataCount,
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
                await ProgressIO.ProgressIO.ExportStateAsync(database);
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