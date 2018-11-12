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
                bool[] initialState = new bool[problem.TransitionFunctionsB.Length];
                for (int i = 0; i < problem.TransitionFunctionsB.Length; i++)
                {
                    if (problem.TransitionFunctionsB[i] != OptionalAutomaton.MissingTransition)
                    {
                        n += 1;
                        initialState[i] = true;
                    }
                }
                var maximumDepth = (n * (n * n - 1)) / 6;
                var shortestPath = int.MaxValue;


                if (checkForSingleton(initialState))
                    shortestPath = 0;
                else
                    CheckRecursively(initialState, 1);



                var solved = new SolvedOptionalAutomaton();
                if (shortestPath == int.MaxValue)
                {
                    solved.SetSolution(problem, null);
                }
                else
                {
                    solved.SetSolution(problem, (ushort)shortestPath);
                }
                yield return solved;
                bool checkForSingleton(bool[] array) => array.Count(e => e) == 1;
                void CheckRecursively(bool[] consideringState, int depth)
                {


                    // depth means the length of word after transition inside

                    // TODO: verify boundary conditions
                    if (depth < shortestPath && depth <= maximumDepth)
                    {
                        var nextState = new bool[consideringState.Length];

                        for (int i = 0; i < consideringState.Length; i++)
                            if (consideringState[i])
                                nextState[problem.TransitionFunctionsA[i]] = true;

                        if (checkForSingleton(nextState))
                        {
                            shortestPath = depth;
                            return;
                        }

                        CheckRecursively(nextState, depth + 1);

                        Array.Clear(nextState, 0, nextState.Length);

                        for (int i = 0; i < consideringState.Length; i++)
                            if (consideringState[i])
                                nextState[problem.TransitionFunctionsB[i]] = true;

                        if (checkForSingleton(nextState))
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
