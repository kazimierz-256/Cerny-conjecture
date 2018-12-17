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

        public UnaryAutomataHub(UnaryAutomataDB database)
        {
            this.database = database;
        }

        public async Task ReceiveSolvedUnaryAutomatonAndAskForMore(long uniqueID, List<ISolvedOptionalAutomaton> solvedInterestingAutomata, int nextQuantity)
        {
            var validID = database.MarkAsSolvedAutomata(uniqueID);
            if (validID)
            {
                // add solved automata to some database or something
                foreach (var automaton in solvedInterestingAutomata)
                {
                    database.AddInterestingAutomaton(automaton);
                }

                if (nextQuantity > 0)
                    await SendUnaryAutomataIndices(nextQuantity);
            }
            else
            {
                // wrong id, suspicious...
                await Clients.Caller.SendAsync("NoMoreAutomataThankYou");
            }
        }

        public async Task SendUnaryAutomataIndices(int quantity)
        {
            // no need to limit the quantity (in case of overflow automata are being recomputed)
            var limitedQuantity = quantity;//Math.Min(512, quantity);
            database.GenerateNewPacket(
                limitedQuantity,
                out var newPacketID,
                out var automataPacket
                );

            if (automataPacket.Length > 0)
            {
                await Clients.Caller.SendAsync("ComputeAutomata",
                    newPacketID,
                    database.Size,
                    automataPacket
                    );
            }
            else
            {
                await Clients.Caller.SendAsync("NoMoreAutomataThankYou");
            }
        }
    }
}