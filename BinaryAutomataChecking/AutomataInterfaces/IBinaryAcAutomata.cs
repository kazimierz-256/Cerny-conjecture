using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public interface IBinaryAcAutomata : CoreDefinitions.IOptionalAutomaton
    {
        IEnumerable<IBinaryAutomata> MakeFullAutomatas();

        byte AcSize { get; }
    }
}
