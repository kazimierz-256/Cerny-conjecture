using System;
using System.Threading.Tasks;

namespace CoreServer.SignalRInterfaces
{
    public interface IPresentation
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
    }
}
