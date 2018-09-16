using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public interface IUnaryAutomata
    {
        byte[] TransitionFunctionsA { get; }
        IEnumerable<IBinaryAcAutomata> MakeAcAutomatas();
    }
}
