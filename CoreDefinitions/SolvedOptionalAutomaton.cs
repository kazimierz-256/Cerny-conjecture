using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public class SolvedOptionalAutomaton : OptionalAutomaton, ISolvedOptionalAutomaton
    {
        public SolvedOptionalAutomaton(IOptionalAutomaton automaton, bool isSynchronizable, ushort synchronizingWordLength) : base(automaton.TransitionFunctionsA, automaton.TransitionFunctionsB)
        {
            IsSynchronizable = isSynchronizable;
            SynchronizingWordLength = synchronizingWordLength;
        }

        public ushort SynchronizingWordLength { get; private set; }
        public bool IsSynchronizable { get; private set; }
    }
}
