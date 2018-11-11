using System;
using System.Collections.Generic;
using System.Text;
using CoreDefinitions;

namespace AutomataIterator
{
    public class PowerAutomatonReusableSolutionMapperSlowBruteForce : ISolutionMapperReusable
    {
        public IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved(IEnumerable<IOptionalAutomaton> problemsToSolve) => throw new NotImplementedException();
    }
}
