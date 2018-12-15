using CoreServer.SignalRInterfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.Hubs
{
    public class SummarizingHub : Hub<IPresentation>
    {
        public async Task SendStatistics(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }
    }
}
