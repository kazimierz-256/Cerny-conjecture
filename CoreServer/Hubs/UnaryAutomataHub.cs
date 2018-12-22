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

        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(List<int> unarySolved, List<byte[]> toSendSolvedUnary, List<List<ushort>> toSendSolvedSyncLength, List<List<byte[]>> toSendSolvedB, int nextQuantity)
        {
            database.ProcessInterestingAutomata(unarySolved, toSendSolvedUnary, toSendSolvedSyncLength, toSendSolvedB, out var changedMinimum);

            if (changedMinimum)
                await Clients.Others.SendAsync("UpdateLength", database.MinimalLength);

            if (nextQuantity > 0)
                await SendUnaryAutomataIndices(nextQuantity);
        }

        public async Task SendUnaryAutomataIndices(int quantity)
        {
            // no need to limit the quantity (in case of overflow automata are being recomputed)
            var automataIndices = database.GetUnaryAutomataToProcessAndMarkAsProcessing(quantity);
            if (automataIndices.Count > 0)
            {
                await Clients.Caller.SendAsync("ComputeAutomata", database.Size, database.MinimalLength, automataIndices);
            }
            else
            {
                await Clients.Caller.SendAsync("NoMoreAutomataThankYou");
            }
        }
    }
}