using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public class SolvedOptionalAutomaton : OptionalAutomaton, ISolvedOptionalAutomaton
    {
        public SolvedOptionalAutomaton(byte[] TransitionFunctionsA, byte[] TransitionFunctionsB, bool isSynchronizable, ushort synchronizingWordLength) : base(TransitionFunctionsA, TransitionFunctionsB)
        {
            IsSynchronizable = isSynchronizable;
            SynchronizingWordLength = synchronizingWordLength;
        }

        public ushort SynchronizingWordLength { get; private set; }
        public bool IsSynchronizable { get; private set; }
    }
}
