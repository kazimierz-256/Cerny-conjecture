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

        private const string solversGroup = "solvers";
        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(ClientServerRequestForMoreAutomata parameters)
        {
            // answer for different automaton size!
            if (parameters.solution.unaryArray.Length != database.Size)
                return;

            database.ProcessInterestingAutomata(parameters, Context.ConnectionId, out var changedMinimum, out var changedAnything);

            if (changedMinimum)
                await Clients.Group(solversGroup).SendAsync("UpdateLength", database.MinimalLength);


            if (parameters.nextQuantity > 0)
            {
                // no need to limit the quantity (in case of overflow automata are being recomputed)
                var hasAutomata = database.GetUnaryAutomataToProcessAndMarkAsProcessing(out var automatonIndex);
                if (hasAutomata)
                {
                    var parameter = new ServerClientSentUnaryAutomataWithSettings()
                    {
                        automatonSize = database.Size,
                        serverMinimalLength = database.MinimalLength,
                        unaryAutomatonIndex = automatonIndex,
                        targetCollectionSize = database.MaximumLongestAutomataCount
                    };
                    await Clients.Caller.SendAsync("ComputeAutomata", parameter);
                }
                else
                {
                    await Clients.Group(solversGroup).SendAsync("NoMoreAutomataThankYou");
                }
            }

            if (changedAnything)
            {
                await SendStatisticsToAll();
                await ProgressIO.ProgressIO.ExportStateAsync(database);
            }
        }

        public async Task InitializeConnection()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, solversGroup);

            // no need to limit the quantity (in case of overflow automata are being recomputed)
            var hasAutomata = database.GetUnaryAutomataToProcessAndMarkAsProcessing(out var automatonIndex);
            if (hasAutomata)
            {
                var parameter = new ServerClientSentUnaryAutomataWithSettings()
                {
                    automatonSize = database.Size,
                    serverMinimalLength = database.MinimalLength,
                    unaryAutomatonIndex = automatonIndex,
                    targetCollectionSize = database.MaximumLongestAutomataCount
                };
                await Clients.Caller.SendAsync("ComputeAutomata", parameter);
            }
            else
            {
                await Clients.Group(solversGroup).SendAsync("NoMoreAutomataThankYou");
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