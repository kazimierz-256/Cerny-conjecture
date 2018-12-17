using CoreServer.UnaryAutomataDatabase;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.Hubs
{
    public class SummarizingHub : Hub
    {
        private readonly UnaryAutomataDB database;

        public SummarizingHub(UnaryAutomataDB database)
        {
            this.database = database;
        }

        public async Task SendStatistics()
        {
            await Clients.Caller.SendAsync("ShowSimpleTextStatistics", database.DumpStatistics());
        }
    }
}
