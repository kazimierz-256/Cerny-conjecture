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

        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(ClientServerRequestForMoreAutomata parameters)
        {
            database.ProcessInterestingAutomata(parameters, out var changedMinimum);

            if (changedMinimum)
                await Clients.Others.SendAsync("UpdateLength", database.MinimalLength);

            if (parameters.nextQuantity > 0)
                await SendUnaryAutomataIndices(parameters.nextQuantity);
        }

        public async Task SendUnaryAutomataIndices(int quantity)
        {
            // no need to limit the quantity (in case of overflow automata are being recomputed)
            var automataIndices = database.GetUnaryAutomataToProcessAndMarkAsProcessing(quantity);
            if (automataIndices.Count > 0)
            {
                var parameter = new ServerClientSentUnaryAutomataWithSettings()
                {
                    automatonSize = database.Size,
                    serverMinimalLength = database.MinimalLength,
                    unaryAutomataIndices = automataIndices,
                    targetCollectionSize = UnaryAutomataDB.MaximumLongestAutomataCount
                };
                await Clients.Caller.SendAsync("ComputeAutomata", parameter);
            }
            else
            {
                await Clients.Caller.SendAsync("NoMoreAutomataThankYou");
            }
        }
    }
}