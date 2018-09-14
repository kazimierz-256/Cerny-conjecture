using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public class SolvedOptionalAutomaton : OptionalAutomaton, ISolvedOptionalAutomaton
    {
        public SolvedOptionalAutomaton() : base(new byte[0], new byte[0])
        {
        }

        public SolvedOptionalAutomaton(IOptionalAutomaton automaton, ushort? synchronizingWordLength) : base(automaton.TransitionFunctionsA, automaton.TransitionFunctionsB)
        {
            SynchronizingWordLength = synchronizingWordLength;
        }

        public ushort? SynchronizingWordLength { get; private set; } = null;

        public void SetSolution(IOptionalAutomaton automaton, ushort? synchronizingWordLength)
        {
            TransitionFunctionsA = automaton.TransitionFunctionsA;
            TransitionFunctionsB = automaton.TransitionFunctionsB;
            SynchronizingWordLength = synchronizingWordLength;
        }
    }
}
