using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomataIterator
{
    public static class ExtensionMapProblemsToSolutions
    {
        private static Dictionary<Type, ISolutionMapperReusable> cachedSet = new Dictionary<Type, ISolutionMapperReusable>();

        public static IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved<T>(this IEnumerable<IOptionalAutomaton> problemsToSolve) where T : ISolutionMapperReusable, new()
        {
            if (!cachedSet.ContainsKey(typeof(T)))
            {
                lock (cachedSet)
                {
                    if (!cachedSet.ContainsKey(typeof(T)))
                    {
                        cachedSet.Add(typeof(T), new T());
                    }
                }
            }

            return cachedSet[typeof(T)].SelectAsSolved(problemsToSolve);
        }

        public static IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved(this IEnumerable<IOptionalAutomaton> problemsToSolve)
            => SelectAsSolved<PowerAutomatonReusableSolutionMapperUltraMaximum12>(problemsToSolve);

        public static ISolutionMapperReusable GetNewMapper() => new PowerAutomatonReusableSolutionMapperUltraMaximum12();
    }
}
