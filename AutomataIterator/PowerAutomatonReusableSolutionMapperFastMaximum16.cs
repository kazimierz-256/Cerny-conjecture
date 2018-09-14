using CoreDefinitions;
using System;
using System.Collections.Generic;

namespace AutomataIterator
{
    /// <summary>
    /// This class is and should be reused whenever possible to avoid the cost of array reallocation
    /// </summary>
    public class PowerAutomatonReusableSolutionMapperFastMaximum16 : ISolutionMapperReusable
    {
        private readonly SolvedOptionalAutomaton automatonToYield = new SolvedOptionalAutomaton();
        /// <summary>
        /// Purposefully this is a constant! The performance is greatly increased!
        /// </summary>
        private const byte maxAutomatonSize = 16;
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
            byte i;
            byte iPower;
            byte max;

            var twoToPowerBits = (byte)(1 << bits);
            var twoToPowerBitsM1 = (byte)((1 << bits) - 1);
            byte iMax = ((maxAutomatonSize + bits - 1) / bits);
            uint tmpTransition;
            uint vertexAfterTransition;
            var precomputedStateTransitioningMatrix = new uint[maxAutomatonSize];
            var transitionMatrixCombined = new uint[twoToPowerBits * iMax];

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


                for (i = 0; i < max; i += 1)
                {
                    if (transitionB[i] != OptionalAutomaton.MissingTransition)
                    {
                        initialVertex += (ushort)(1 << i);

                        precomputedStateTransitioningMatrix[i] = (uint)((powerSetCount << transitionA[i]) + (1 << transitionB[i]));
                    }
                    else
                    {
                        // not sure if this is necessary?
                        //precomputedStateTransitioningMatrix[i] = 0;
                    }
                }

                for (i = 0, iPower = 0; i < max; i += bits, iPower += twoToPowerBits)
                {
                    transitionMatrixCombined[iPower + 0b0001]
                      = transitionMatrixCombined[iPower + 0b0011]
                      = transitionMatrixCombined[iPower + 0b0101]
                      = transitionMatrixCombined[iPower + 0b0111]
                      = transitionMatrixCombined[iPower + 0b1001]
                      = transitionMatrixCombined[iPower + 0b1011]
                      = transitionMatrixCombined[iPower + 0b1101]
                      = transitionMatrixCombined[iPower + 0b1111]
                      = precomputedStateTransitioningMatrix[i];

                    if (i >= maxAutomatonSize - 1)
                        break;
                    tmpTransition = precomputedStateTransitioningMatrix[i + 1];
                    transitionMatrixCombined[iPower + 0b0010] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b0011] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b0110] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b0111] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1010] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b1011] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1110] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b1111] |= tmpTransition;

                    if (i >= maxAutomatonSize - 2)
                        break;
                    tmpTransition = precomputedStateTransitioningMatrix[i + 2];
                    transitionMatrixCombined[iPower + 0b0100] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b0101] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b0110] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b0111] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1100] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b1101] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1110] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1111] |= tmpTransition;

                    if (i >= maxAutomatonSize - 3)
                        break;
                    tmpTransition = precomputedStateTransitioningMatrix[i + 3];
                    transitionMatrixCombined[iPower + 0b1000] = tmpTransition;
                    transitionMatrixCombined[iPower + 0b1001] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1010] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1011] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1100] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1101] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1110] |= tmpTransition;
                    transitionMatrixCombined[iPower + 0b1111] |= tmpTransition;
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

                    vertexAfterTransition =
                      transitionMatrixCombined[twoToPowerBitsM1 & consideringVertex]
                    | transitionMatrixCombined[twoToPowerBits + (twoToPowerBitsM1 & (consideringVertex >> bits))]
                    | transitionMatrixCombined[2 * twoToPowerBits + (twoToPowerBitsM1 & (consideringVertex >> (2 * bits)))]
                    | transitionMatrixCombined[3 * twoToPowerBits + (twoToPowerBitsM1 & (consideringVertex >> (3 * bits)))];

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

