using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface ISolvedOptionalAutomaton : IOptionalAlgorithmDefinition
    {
        ushort SynchronizingWordLength { get; }
        bool IsSynchronizable { get; }
    }
}
