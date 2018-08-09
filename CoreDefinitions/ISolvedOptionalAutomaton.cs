using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface ISolvedOptionalAutomaton : IOptionalAutomaton
    {
        ushort SynchronizingWordLength { get; }
        bool IsSynchronizable { get; }
    }
}
