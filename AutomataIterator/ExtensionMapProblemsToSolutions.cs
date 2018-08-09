using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomataIterator
{
    public static class ExtensionMapProblemsToSolutions
    {
        public static IEnumerable<ISolvedOptionalAutomaton> MapToPowerAutomatonSolution<T>(this IEnumerable<IOptionalAutomaton> problemsToSolve) where T : ISolutionMapperReusable, new()
            => new T().MapToSolvedAutomaton(problemsToSolve);

    }
}
