using CoreDefinitions;
using System;
using System.Collections.Generic;

namespace AutomataIterator
{
    /// <summary>
    /// This class is and should be reused whenever possible to avoid the cost of array reallocation
    /// </summary>
    public class PowerAutomatonReusableSolutionMapperUltraMaximum12 : ISolutionMapperReusable
    {
        private readonly SolvedOptionalAutomaton automatonToYield = new SolvedOptionalAutomaton();
        /// <summary>
        /// Purposefully this is a constant! The performance is greatly increased!
        /// </summary>
        private const byte maxAutomatonSize = 12;
        private const byte bits = 4;

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

            ushort consideringVertex;
            ushort vertexAfterTransitionA;
            ushort vertexAfterTransitionB;

            ushort initialVertex;
            ushort currentNextDistance;
            ushort verticesUntilBump;
            bool seekingFirstNext;
            bool discoveredSingleton;
            byte i;
            byte max;

            const byte twoToPowerBits = 1 << bits;
            const byte twoToPowerBitsM1 = (1 << bits) - 1;
            uint tmpTransition;
            uint vertexAfterTransition;
            var precomputedStateTransitioningMatrix = new uint[maxAutomatonSize];
            var transitionMatrixCombined1 = new uint[twoToPowerBits];
            var transitionMatrixCombined2 = new uint[twoToPowerBits];
            var transitionMatrixCombined3 = new uint[twoToPowerBits];
            var transitionMatrixCombined4 = new uint[twoToPowerBits];

            foreach (var automaton in problemsToSolve)
            {
                var transitionA = automaton.TransitionFunctionsA;
                var transitionB = automaton.TransitionFunctionsB;
                max = (byte)transitionB.Length;
                if (max > maxAutomatonSize)
                {
                    throw new Exception($"Automaton is too large (maximum supported: {maxAutomatonSize}, given: {max})");
                }

                localProblemId += 1;

                // that's improbable since 2^32-1 is a very large number of problems
                if (localProblemId <= 0)
                {
                    localProblemId = 1;
                    Array.Clear(isDiscovered, 0, isDiscovered.Length);
                }
                readingIndex = 0;
                writingIndex = 1;

                initialVertex = 0;


                for (i = 0; i < max; i += 1)
                {
                    if (transitionB[i] != OptionalAutomaton.MissingTransition)
                    {
                        initialVertex += (ushort)(1 << i);
                        precomputedStateTransitioningMatrix[i] = (uint)((powerSetCount << transitionA[i]) + (1 << transitionB[i]));
                    }
                }

                if (0 == (initialVertex & (initialVertex - 1)))
                {
                    automatonToYield.SetSolution(automaton, 0);
                    yield return automatonToYield;
                    continue;
                }


                transitionMatrixCombined1[0b0001]
                  = transitionMatrixCombined1[0b0011]
                  = transitionMatrixCombined1[0b0101]
                  = transitionMatrixCombined1[0b0111]
                  = transitionMatrixCombined1[0b1001]
                  = transitionMatrixCombined1[0b1011]
                  = transitionMatrixCombined1[0b1101]
                  = transitionMatrixCombined1[0b1111]
                  = precomputedStateTransitioningMatrix[0];
                if (bits < maxAutomatonSize)
                {
                    transitionMatrixCombined2[0b0001]
                  = transitionMatrixCombined2[0b0011]
                  = transitionMatrixCombined2[0b0101]
                  = transitionMatrixCombined2[0b0111]
                  = transitionMatrixCombined2[0b1001]
                  = transitionMatrixCombined2[0b1011]
                  = transitionMatrixCombined2[0b1101]
                  = transitionMatrixCombined2[0b1111]
                  = precomputedStateTransitioningMatrix[bits];
                    if (bits * 2 < maxAutomatonSize)
                    {
                        transitionMatrixCombined3[0b0001]
                      = transitionMatrixCombined3[0b0011]
                      = transitionMatrixCombined3[0b0101]
                      = transitionMatrixCombined3[0b0111]
                      = transitionMatrixCombined3[0b1001]
                      = transitionMatrixCombined3[0b1011]
                      = transitionMatrixCombined3[0b1101]
                      = transitionMatrixCombined3[0b1111]
                      = precomputedStateTransitioningMatrix[bits * 2];
                        if (bits * 3 < maxAutomatonSize)
                        {
                            transitionMatrixCombined4[0b0001]
                              = transitionMatrixCombined4[0b0011]
                              = transitionMatrixCombined4[0b0101]
                              = transitionMatrixCombined4[0b0111]
                              = transitionMatrixCombined4[0b1001]
                              = transitionMatrixCombined4[0b1011]
                              = transitionMatrixCombined4[0b1101]
                              = transitionMatrixCombined4[0b1111]
                              = precomputedStateTransitioningMatrix[bits * 3];
                        }
                    }
                }

                if (1 < maxAutomatonSize)
                {
                    tmpTransition = precomputedStateTransitioningMatrix[0 + 1];
                    transitionMatrixCombined1[0b0010] = tmpTransition;
                    transitionMatrixCombined1[0b0011] |= tmpTransition;
                    transitionMatrixCombined1[0b0110] = tmpTransition;
                    transitionMatrixCombined1[0b0111] |= tmpTransition;
                    transitionMatrixCombined1[0b1010] = tmpTransition;
                    transitionMatrixCombined1[0b1011] |= tmpTransition;
                    transitionMatrixCombined1[0b1110] = tmpTransition;
                    transitionMatrixCombined1[0b1111] |= tmpTransition;
                    if (bits + 1 < maxAutomatonSize)
                    {
                        tmpTransition = precomputedStateTransitioningMatrix[bits + 1];
                        transitionMatrixCombined2[0b0010] = tmpTransition;
                        transitionMatrixCombined2[0b0011] |= tmpTransition;
                        transitionMatrixCombined2[0b0110] = tmpTransition;
                        transitionMatrixCombined2[0b0111] |= tmpTransition;
                        transitionMatrixCombined2[0b1010] = tmpTransition;
                        transitionMatrixCombined2[0b1011] |= tmpTransition;
                        transitionMatrixCombined2[0b1110] = tmpTransition;
                        transitionMatrixCombined2[0b1111] |= tmpTransition;
                        if (bits * 2 + 1 < maxAutomatonSize)
                        {
                            tmpTransition = precomputedStateTransitioningMatrix[bits * 2 + 1];
                            transitionMatrixCombined3[0b0010] = tmpTransition;
                            transitionMatrixCombined3[0b0011] |= tmpTransition;
                            transitionMatrixCombined3[0b0110] = tmpTransition;
                            transitionMatrixCombined3[0b0111] |= tmpTransition;
                            transitionMatrixCombined3[0b1010] = tmpTransition;
                            transitionMatrixCombined3[0b1011] |= tmpTransition;
                            transitionMatrixCombined3[0b1110] = tmpTransition;
                            transitionMatrixCombined3[0b1111] |= tmpTransition;
                            if (bits * 3 + 1 < maxAutomatonSize)
                            {
                                tmpTransition = precomputedStateTransitioningMatrix[bits * 3 + 1];
                                transitionMatrixCombined4[0b0010] = tmpTransition;
                                transitionMatrixCombined4[0b0011] |= tmpTransition;
                                transitionMatrixCombined4[0b0110] = tmpTransition;
                                transitionMatrixCombined4[0b0111] |= tmpTransition;
                                transitionMatrixCombined4[0b1010] = tmpTransition;
                                transitionMatrixCombined4[0b1011] |= tmpTransition;
                                transitionMatrixCombined4[0b1110] = tmpTransition;
                                transitionMatrixCombined4[0b1111] |= tmpTransition;
                            }
                        }
                    }

                    if (2 < maxAutomatonSize)
                    {
                        tmpTransition = precomputedStateTransitioningMatrix[0 + 2];
                        transitionMatrixCombined1[0b0100] = tmpTransition;
                        transitionMatrixCombined1[0b0101] |= tmpTransition;
                        transitionMatrixCombined1[0b0110] |= tmpTransition;
                        transitionMatrixCombined1[0b0111] |= tmpTransition;
                        transitionMatrixCombined1[0b1100] = tmpTransition;
                        transitionMatrixCombined1[0b1101] |= tmpTransition;
                        transitionMatrixCombined1[0b1110] |= tmpTransition;
                        transitionMatrixCombined1[0b1111] |= tmpTransition;
                        if (bits + 2 < maxAutomatonSize)
                        {
                            tmpTransition = precomputedStateTransitioningMatrix[bits + 2];
                            transitionMatrixCombined2[0b0100] = tmpTransition;
                            transitionMatrixCombined2[0b0101] |= tmpTransition;
                            transitionMatrixCombined2[0b0110] |= tmpTransition;
                            transitionMatrixCombined2[0b0111] |= tmpTransition;
                            transitionMatrixCombined2[0b1100] = tmpTransition;
                            transitionMatrixCombined2[0b1101] |= tmpTransition;
                            transitionMatrixCombined2[0b1110] |= tmpTransition;
                            transitionMatrixCombined2[0b1111] |= tmpTransition;
                            if (bits * 2 + 2 < maxAutomatonSize)
                            {
                                tmpTransition = precomputedStateTransitioningMatrix[bits * 2 + 2];
                                transitionMatrixCombined3[0b0100] = tmpTransition;
                                transitionMatrixCombined3[0b0101] |= tmpTransition;
                                transitionMatrixCombined3[0b0110] |= tmpTransition;
                                transitionMatrixCombined3[0b0111] |= tmpTransition;
                                transitionMatrixCombined3[0b1100] = tmpTransition;
                                transitionMatrixCombined3[0b1101] |= tmpTransition;
                                transitionMatrixCombined3[0b1110] |= tmpTransition;
                                transitionMatrixCombined3[0b1111] |= tmpTransition;
                                if (bits * 3 + 2 < maxAutomatonSize)
                                {
                                    tmpTransition = precomputedStateTransitioningMatrix[bits * 3 + 2];
                                    transitionMatrixCombined4[0b0100] = tmpTransition;
                                    transitionMatrixCombined4[0b0101] |= tmpTransition;
                                    transitionMatrixCombined4[0b0110] |= tmpTransition;
                                    transitionMatrixCombined4[0b0111] |= tmpTransition;
                                    transitionMatrixCombined4[0b1100] = tmpTransition;
                                    transitionMatrixCombined4[0b1101] |= tmpTransition;
                                    transitionMatrixCombined4[0b1110] |= tmpTransition;
                                    transitionMatrixCombined4[0b1111] |= tmpTransition;
                                }
                            }
                        }

                        if (3 < maxAutomatonSize)
                        {
                            tmpTransition = precomputedStateTransitioningMatrix[0 + 3];
                            transitionMatrixCombined1[0b1000] = tmpTransition;
                            transitionMatrixCombined1[0b1001] |= tmpTransition;
                            transitionMatrixCombined1[0b1010] |= tmpTransition;
                            transitionMatrixCombined1[0b1011] |= tmpTransition;
                            transitionMatrixCombined1[0b1100] |= tmpTransition;
                            transitionMatrixCombined1[0b1101] |= tmpTransition;
                            transitionMatrixCombined1[0b1110] |= tmpTransition;
                            transitionMatrixCombined1[0b1111] |= tmpTransition;
                            if (bits + 3 < maxAutomatonSize)
                            {
                                tmpTransition = precomputedStateTransitioningMatrix[bits + 3];
                                transitionMatrixCombined2[0b1000] = tmpTransition;
                                transitionMatrixCombined2[0b1001] |= tmpTransition;
                                transitionMatrixCombined2[0b1010] |= tmpTransition;
                                transitionMatrixCombined2[0b1011] |= tmpTransition;
                                transitionMatrixCombined2[0b1100] |= tmpTransition;
                                transitionMatrixCombined2[0b1101] |= tmpTransition;
                                transitionMatrixCombined2[0b1110] |= tmpTransition;
                                transitionMatrixCombined2[0b1111] |= tmpTransition;
                                if (bits * 2 + 3 < maxAutomatonSize)
                                {
                                    tmpTransition = precomputedStateTransitioningMatrix[bits * 2 + 3];
                                    transitionMatrixCombined3[0b1000] = tmpTransition;
                                    transitionMatrixCombined3[0b1001] |= tmpTransition;
                                    transitionMatrixCombined3[0b1010] |= tmpTransition;
                                    transitionMatrixCombined3[0b1011] |= tmpTransition;
                                    transitionMatrixCombined3[0b1100] |= tmpTransition;
                                    transitionMatrixCombined3[0b1101] |= tmpTransition;
                                    transitionMatrixCombined3[0b1110] |= tmpTransition;
                                    transitionMatrixCombined3[0b1111] |= tmpTransition;
                                    if (bits * 3 + 3 < maxAutomatonSize)
                                    {
                                        tmpTransition = precomputedStateTransitioningMatrix[bits * 3 + 3];
                                        transitionMatrixCombined4[0b1000] = tmpTransition;
                                        transitionMatrixCombined4[0b1001] |= tmpTransition;
                                        transitionMatrixCombined4[0b1010] |= tmpTransition;
                                        transitionMatrixCombined4[0b1011] |= tmpTransition;
                                        transitionMatrixCombined4[0b1100] |= tmpTransition;
                                        transitionMatrixCombined4[0b1101] |= tmpTransition;
                                        transitionMatrixCombined4[0b1110] |= tmpTransition;
                                        transitionMatrixCombined4[0b1111] |= tmpTransition;
                                    }
                                }
                            }
                        }
                    }
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

                    if (maxAutomatonSize > 12)
                    {
                        vertexAfterTransition =
                          transitionMatrixCombined1[twoToPowerBitsM1 & consideringVertex]
                        | transitionMatrixCombined2[twoToPowerBitsM1 & (consideringVertex >> bits)]
                        | transitionMatrixCombined3[twoToPowerBitsM1 & (consideringVertex >> (2 * bits))]
                        | transitionMatrixCombined4[twoToPowerBitsM1 & (consideringVertex >> (3 * bits))];
                    }
                    else
                    {
                        vertexAfterTransition =
                          transitionMatrixCombined1[twoToPowerBitsM1 & consideringVertex]
                        | transitionMatrixCombined2[twoToPowerBitsM1 & (consideringVertex >> bits)]
                        | transitionMatrixCombined3[twoToPowerBitsM1 & (consideringVertex >> (2 * bits))];
                    }

                    vertexAfterTransitionA = (ushort)(vertexAfterTransition >> maxAutomatonSize);
                    vertexAfterTransitionB = (ushort)(vertexAfterTransition & initialVertex);

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

