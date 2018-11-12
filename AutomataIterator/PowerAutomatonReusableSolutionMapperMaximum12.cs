using CoreDefinitions;
using System;
using System.Collections.Generic;

namespace AutomataIterator
{
    /// <summary>
    /// This class is and should be reused whenever possible to avoid the cost of array reallocation
    /// </summary>
    public class PowerAutomatonReusableSolutionMapperMaximum12 : ISolutionMapperReusable
    {
        private readonly SolvedOptionalAutomaton automatonToYield = new SolvedOptionalAutomaton();
        /// <summary>
        /// Purposefully this is a constant! The performance is greatly increased!
        /// </summary>
        private const int maxAutomatonSize = 12;
        private readonly static byte n2 = maxAutomatonSize << 1;
        private readonly static byte n2m1 = (byte)(n2 - 1);
        private readonly static int powerSetCount = 1 << maxAutomatonSize;
        private uint localProblemId = 0;
        private readonly uint[] isDiscovered = new uint[powerSetCount];
        private readonly ushort[] queue = new ushort[powerSetCount];

        /// <summary>
        /// Maps automata to their solutions (are they synchronizable?, what is the length of synchronizing word?)
        /// Initial vertex is designed to be the superposition of all defined vertices (and only them)
        /// </summary>
        /// <param name="problemsToSolve"></param>
        /// <returns></returns>
        public IEnumerable<ISolvedOptionalAutomaton> SelectAsSolved(IEnumerable<IOptionalAutomaton> problemsToSolve)
        {
            ushort readingIndex = 0;
            ushort writingIndex = 0;

            var precomputedStateTransitioningMatrixA = new ushort[maxAutomatonSize];
            var precomputedStateTransitioningMatrixB = new ushort[maxAutomatonSize];

            ushort consideringVertex;
            ushort vertexAfterTransitionA;
            ushort vertexAfterTransitionB;

            ushort initialVertex;
            ushort currentNextDistance;
            ushort verticesUntilBump;
            bool seekingFirstNext;
            bool discoveredSingleton;
            byte vertex;
            ushort mask;
            int max;

            foreach (var automaton in problemsToSolve)
            {
                var transitionA = automaton.TransitionFunctionsA;
                var transitionB = automaton.TransitionFunctionsB;
                max = (byte)transitionB.Length;

                localProblemId += 1;

                // that's unprobable since 2^32-1 is a very large number of problems
                if (localProblemId <= 0)
                {
                    localProblemId = 1;
                    Array.Clear(isDiscovered, 0, isDiscovered.Length);
                    //// should be faster than zeroing out an array
                    //isDiscovered = new byte[powerSetCount];
                }
                readingIndex = 0;
                writingIndex = 1;

                initialVertex = 0;
                for (vertex = 0; vertex < max; vertex += 1)
                {
                    if (transitionB[vertex] != OptionalAutomaton.MissingTransition)
                    {
                        initialVertex += (ushort)(1 << vertex);
                        precomputedStateTransitioningMatrixA[vertex] = (ushort)(1 << transitionA[vertex]);
                        precomputedStateTransitioningMatrixB[vertex] = (ushort)(1 << transitionB[vertex]);
                    }
                }

                if (0 == (initialVertex & (initialVertex - 1)))
                {
                    automatonToYield.SetSolution(automaton, 0);
                    yield return automatonToYield;
                    continue;
                }

                isDiscovered[initialVertex] = localProblemId;
                queue[0] = initialVertex;
                currentNextDistance = 1;
                verticesUntilBump = ushort.MaxValue;
                seekingFirstNext = true;
                discoveredSingleton = false;

                while (readingIndex < writingIndex)
                {
                    consideringVertex = queue[readingIndex];
                    readingIndex += 1;

                    if ((verticesUntilBump -= 1) == 0)
                    {
                        currentNextDistance += 1;
                        seekingFirstNext = true;
                    }

                    vertexAfterTransitionA = vertexAfterTransitionB = 0;

                    // watch out for the index range in the for loop
                    mask = 1;
                    for (vertex = 0; vertex < maxAutomatonSize; vertex += 1, mask <<= 1)
                    {
                        if (0 != (mask & consideringVertex))
                        {
                            vertexAfterTransitionA |= precomputedStateTransitioningMatrixA[vertex];
                            vertexAfterTransitionB |= precomputedStateTransitioningMatrixB[vertex];
                        }
                    }

                    // computed vertex was never discovered
                    if (localProblemId != isDiscovered[vertexAfterTransitionA])
                    {
                        // only one bit is set (no need to check that vertex is 0)
                        if (0 == (vertexAfterTransitionA & (vertexAfterTransitionA - 1)))
                        {
                            discoveredSingleton = true;
                            break;
                        }

                        // this vertex is now marked as discovered
                        isDiscovered[vertexAfterTransitionA] = localProblemId;
                        // the vertex is being appended to the queue
                        queue[writingIndex] = vertexAfterTransitionA;
                        writingIndex += 1;

                        // found a representative vertex of next BFS iteration, now preparing for the upcoming next iteration
                        if (seekingFirstNext)
                        {
                            seekingFirstNext = false;
                            verticesUntilBump = (ushort)(writingIndex - readingIndex);
                        }
                    }

                    // computed vertex was never discovered
                    if (localProblemId != isDiscovered[vertexAfterTransitionB])
                    {
                        // only one bit is set (no need to check that vertex is 0)
                        if (0 == (vertexAfterTransitionB & (vertexAfterTransitionB - 1)))
                        {
                            discoveredSingleton = true;
                            break;
                        }

                        // this vertex is now marked as discovered
                        isDiscovered[vertexAfterTransitionB] = localProblemId;
                        // the vertex is being appended to the queue
                        queue[writingIndex] = vertexAfterTransitionB;
                        writingIndex += 1;

                        // found a representative vertex of next BFS iteration, now preparing for the upcoming next iteration
                        if (seekingFirstNext)
                        {
                            seekingFirstNext = false;
                            verticesUntilBump = (ushort)(writingIndex - readingIndex);
                        }
                    }
                }

                if (discoveredSingleton)
                    automatonToYield.SetSolution(automaton, currentNextDistance);
                else
                    automatonToYield.SetSolution(automaton, null);

                yield return automatonToYield;
            }
        }
    }
}

