#define optimizeFor16

using CoreDefinitions;
using System;
using System.Collections.Generic;

namespace AutomataIterator
{
    /// <summary>
    /// This class is and should be reused whenever possible to avoid the cost of array reallocation
    /// </summary>
    public class PowerAutomatonFastSolutionMapper : ISolutionMapperReusable
    {
        /// <summary>
        /// Purposefully this is a constant! The performance is greatly increased!
        /// </summary>
        private const byte maxAutomatonSize = 13;
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
        public IEnumerable<ISolvedOptionalAutomaton> MapToSolvedAutomaton(IEnumerable<IOptionalAutomaton> problemsToSolve)
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

            var twoToPowerBits = (byte)(1 << bits);
            byte iMax = ((maxAutomatonSize + bits - 1) / bits);
            uint tmpTransition;
            uint vertexAfterTransition;
            var precomputedStateTransitioningMatrix = new uint[maxAutomatonSize];
            var transitionMatrixCombined = new uint[twoToPowerBits * iMax];

            foreach (var problem in problemsToSolve)
            {

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

                for (i = 0; i < maxAutomatonSize; i += 1)
                {
                    if (problem.IsDefinedVertex(i))
                    {
                        initialVertex += (ushort)(1 << i);

                        precomputedStateTransitioningMatrix[i] = (uint)(
                        (powerSetCount << problem.GetTransitionA(i))
                        + (1 << problem.GetTransitionB(i))
                        );
                    }
                    else
                    {
                        // not sure if this is necessary?
                        //precomputedStateTransitioningMatrix[i] = 0;
                    }
                }

                for (i = 0, iPower = 0; i < maxAutomatonSize; i += bits, iPower += twoToPowerBits)
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

#if (optimizeFor16)
                    vertexAfterTransition = transitionMatrixCombined[15 & consideringVertex]
                    | transitionMatrixCombined[16 + (15 & (consideringVertex >> 4))]
                    | transitionMatrixCombined[32 + (15 & (consideringVertex >> 8))]
                    | transitionMatrixCombined[48 + (15 & (consideringVertex >> 12))];
#else
                    vertexAfterTransition = transitionMatrixCombined[twoToPowerBitsM1 & consideringVertex];
                    if (1 < iMax)
                    {
                        vertexAfterTransition |= transitionMatrixCombined[16 + (twoToPowerBitsM1 & (consideringVertex >> 4))];
                        if (2 < iMax)
                        {
                            vertexAfterTransition |= transitionMatrixCombined[32 + (twoToPowerBitsM1 & (consideringVertex >> 8))];
                            if (3 < iMax)
                            {
                                vertexAfterTransition |= transitionMatrixCombined[48 + (twoToPowerBitsM1 & (consideringVertex >> 12))];
                            }
                        }
                    }
#endif
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

                yield return problem.CreateSolvedObject(discoveredSingleton, currentNextDistance);
            }
        }
    }
}

