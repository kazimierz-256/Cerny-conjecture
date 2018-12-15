using CoreServer.SignalRInterfaces;
using CoreServer.UnaryAutomataDatabase;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.Hubs
{
    public class UnaryAutomataHub : Hub<IComputingClient>
    {
        private readonly UnaryAutomataDB database;

        public UnaryAutomataHub(UnaryAutomataDB database)
        {
            this.database = database;
        }
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }

        public async Task ReceiveSolvedUnaryAutomaton(int automatonIndex, string solution)
        {
            database.MarkAutomatonAsDone(automatonIndex);
            // save results...
        }

        public async Task SendUnaryAutomataIndices(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }
    }
}