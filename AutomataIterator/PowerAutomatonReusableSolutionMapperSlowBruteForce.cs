using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDefinitions;

namespace AutomataIterator
{
    public class PowerAutomatonReusableSolutionMapperSlowBruteForce : ISolutionMapperReusable
    {
        public IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved(IEnumerable<IOptionalAutomaton> problemsToSolve)
        {
            foreach (var problem in problemsToSolve)
            {
                var n = 0;
                var initialState = new HashSet<int>();
                for (int i = 0; i < problem.TransitionFunctionsB.Length; i++)
                {
                    if (problem.TransitionFunctionsB[i] != OptionalAutomaton.MissingTransition)
                    {
                        n += 1;
                        initialState.Add(i);
                    }
                }
                var maximumDepth = (n * (n * n - 1)) / 6;
                var shortestPath = int.MaxValue;

                var random = new Random(0);
                if (initialState.Count == 1)
                    shortestPath = 0;
                else
                    CheckRecursively(initialState, 1);

                var solved = new SolvedOptionalAutomaton();

                if (shortestPath == int.MaxValue)
                    solved.SetSolution(problem, null);
                else
                    solved.SetSolution(problem, (ushort)shortestPath);

                yield return solved;
                void CheckRecursively(HashSet<int> consideringState, int depth)
                {
                    // depth means the length of word after transition inside

                    if (depth < shortestPath && depth <= maximumDepth)
                    {
                        var nextState = new HashSet<int>();

                        var firstTransition = problem.TransitionFunctionsA;
                        var secondTransition = problem.TransitionFunctionsB;
                        if (random.Next(0, 1) == 0)
                        {
                            firstTransition = problem.TransitionFunctionsB;
                            secondTransition = problem.TransitionFunctionsA;
                        }

                        foreach (var candidate in consideringState)
                            nextState.Add(firstTransition[candidate]);

                        if (nextState.Count == 1)
                        {
                            shortestPath = depth;
                            return;
                        }

                        CheckRecursively(nextState, depth + 1);

                        nextState.Clear();

                        foreach (var candidate in consideringState)
                            nextState.Add(secondTransition[candidate]);

                        if (nextState.Count == 1)
                        {
                            shortestPath = depth;
                            return;
                        }

                        CheckRecursively(nextState, depth + 1);
                    }

                }
            }
        }

    }
}
