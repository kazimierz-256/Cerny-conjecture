using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.SignalRInterfaces
{
    public interface IComputingClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
    }
}
