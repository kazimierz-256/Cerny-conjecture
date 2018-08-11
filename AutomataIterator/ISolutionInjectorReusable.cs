using System.Collections.Generic;
using CoreDefinitions;

namespace AutomataIterator
{
    public interface ISolutionMapperReusable
    {
        IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved(IEnumerable<IOptionalAutomaton> problemsToSolve);
    }
}