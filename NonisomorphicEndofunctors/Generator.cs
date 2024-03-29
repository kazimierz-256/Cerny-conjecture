﻿//#define diagnostics

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UniqueUnaryAutomata
{
    public static class Generator
    {
        public readonly static long[] theory = new long[]
        {
            1, 3, 7, 19, 47, 130, 343, 951, 2615, 7318, 20491, 57903, 163898, 466199, 1328993, 3799624, 10884049, 31241170
        };

        private readonly static int[] integrityTable = new int[]
        {
            0,
            0,
            1,
            1,
            1,
            1,
            1,
            2,
            2,
            2,
            3,
            3,
            3,
            4
        };

        private static int[][][] cachedAutomata = new int[0][][];
        private static object sync = new object();
        private static int CompareAutomata(int[] a1, int[] a2)
        {
            for (int i = 0, max = Math.Min(a1.Length, a2.Length); i < max; i++)
            {
                if (a1[i] != a2[i])
                    return a1[i].CompareTo(a2[i]);
            }
            if (a1.Length != a2.Length)
                return a1.Length.CompareTo(a2.Length);
            return 0;
        }
        public static int[] GetUniqueAutomatonFromCached(int size, int index)
        {
            if (size > cachedAutomata.Length)
            {
                lock (sync)
                {
                    if (size > cachedAutomata.Length)
                    {
                        cachedAutomata = EnumerateCollectionsOfNonisomorphicUnaryAutomata().Take(size).ToArray();

                        for (int i = 0; i < cachedAutomata.Length; i++)
                            Array.Sort(cachedAutomata[i], CompareAutomata);
                    }
                }
            }

            if (index < cachedAutomata[size - 1].Length)
            {
                return cachedAutomata[size - 1][index];
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<int[]> GetAllUniqueAutomataOfSize(int size)
        {
            var automataGenerator = EnumerateCollectionsOfNonisomorphicUnaryAutomata();
            // first endofunction is of size 1
            // second is of size 2
            // third is of size 3...
            return automataGenerator.Skip(size - 1).First();
        }

        public static IEnumerable<int[][]> EnumerateCollectionsOfNonisomorphicUnaryAutomata()
        {
            var generatorHelper = new GeneratorHelper();

            var initialAutomata = new int[][]
            {
                new int[]{ 0 }
            };

            yield return initialAutomata;

            int[][] result = initialAutomata;
            int integrityLevelEstimation = 1;

            for (int newPosition = 1; newPosition < theory.Length; newPosition += 1)
            {
#if diagnostics
                Console.WriteLine("New level computing...");
#endif
                generatorHelper.PrepareFor(newPosition);
                if (newPosition < integrityTable.Length)
                {
                    integrityLevelEstimation = Math.Max(integrityLevelEstimation, integrityTable[newPosition]);
                    result = GenerateOneStepFurtherExactly(result, newPosition, theory[newPosition], integrityLevelEstimation, generatorHelper);
                }
                else
                {
                    result = GenerateOneStepFurther(result, newPosition, theory[newPosition], ref integrityLevelEstimation, generatorHelper);
                }

                yield return result;
#if diagnostics
                if (theory[newPosition] != result.Count())
                {
                    throw new Exception("Doesn't agree with theory");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Theory approved!");
                    Console.ResetColor();
                }
                Console.WriteLine();
#endif
            }
        }

        private static int[][] GenerateOneStepFurtherExactly(IEnumerable<int[]> multipleTransitionFunctions, int newPosition, long targetCount, int precomputedIntegrityLevel, GeneratorHelper generatorHelper)
        {
            var automataToReturn = new int[targetCount][];
            int pasteCounter = 0;
            var hashSet = new ConcurrentDictionary<long, object>();
            foreach (var oldTransitionFunction in multipleTransitionFunctions)
            {
                var verticesIncludingNew = newPosition + 1;
                var temporary = new long[verticesIncludingNew];
                var hashes = new long[verticesIncludingNew];
                var mailbox = new List<long>[verticesIncludingNew];
                for (int i = 0; i < mailbox.Length; i++)
                    mailbox[i] = new List<long>();

                foreach (var newTransitionFunction in GenerateNewMutableLightweightFromSmart(oldTransitionFunction, newPosition))
                {
                    generatorHelper.InitializeIteration(newTransitionFunction, hashes, mailbox);
                    var automatonHash = generatorHelper.IterateAndSquashMultiple(newTransitionFunction, hashes, temporary, mailbox, precomputedIntegrityLevel);
                    if (!hashSet.ContainsKey(automatonHash))
                    {
                        if (hashSet.TryAdd(automatonHash, null))
                        {
                            var index = Interlocked.Increment(ref pasteCounter);
                            automataToReturn[index - 1] = (int[])newTransitionFunction.Clone();
                        }
                    }
                }
            }

#if diagnostics
            if (pasteCounter != automataToReturn.Length)
            {
                throw new Exception("Not enough automata produced!");
            }
#endif
            return automataToReturn;
        }

        private static IEnumerable<int[]> GenerateNewMutableLightweightFromSmart(int[] transitionFunctions, int newPosition)
        {
            var extendedTransitionFunctions = new int[newPosition + 1];
            int remembered;
            transitionFunctions.CopyTo(extendedTransitionFunctions, 0);


            for (int newTransitionTarget = 0, max = newPosition + 1; newTransitionTarget < max; newTransitionTarget += 1)
            {
                extendedTransitionFunctions[newPosition] = newTransitionTarget;

                yield return extendedTransitionFunctions;

                for (int i = newTransitionTarget; i < newPosition; i += 1)
                {
                    remembered = extendedTransitionFunctions[i];
                    extendedTransitionFunctions[i] = newPosition;
                    yield return extendedTransitionFunctions;
                    extendedTransitionFunctions[i] = remembered;
                }
            }
        }

        private class SameHashCollisionGroup
        {
            public ConcurrentBag<int> automatonIDs = new ConcurrentBag<int>();
            public int agreedIterations = 0;
        }

        private static int[][] GenerateOneStepFurther(IEnumerable<int[]> multipleTransitionFunctions, int newPosition, long targetCount, ref int maximumEstimatedIntegrityLevel, GeneratorHelper generatorHelper)
        {
            var automataCandidates = multipleTransitionFunctions
                .Select(transitionFunctions => GenerateNewMutableClonesFromSmart(transitionFunctions, newPosition))
                .SelectMany(id => id)
                .ToArray();

            var individualVerticesHashes = new long[automataCandidates.Length][];

            var verticesIncludingNew = newPosition + 1;

            for (int automatonID = 0; automatonID < automataCandidates.Length; automatonID += 1)
            {
                individualVerticesHashes[automatonID] = new long[verticesIncludingNew];
            }

#if diagnostics
            Console.WriteLine($"Total number of automata: {automataCandidates.Length}");
#endif

            var hashToGroup = new Dictionary<long, SameHashCollisionGroup>();

            int estimatedIntegrityLevel = maximumEstimatedIntegrityLevel;

            void insertIdIntoHashbag(Dictionary<long, SameHashCollisionGroup> map, long hash, int id)
            {
                if (!map.ContainsKey(hash))
                    map.TryAdd(hash, new SameHashCollisionGroup()
                    {
                        automatonIDs = new ConcurrentBag<int>(),
                        agreedIterations = estimatedIntegrityLevel
                    });

                map[hash].automatonIDs.Add(id);
            }

            List<long>[] SetupNewMailbox(int size)
            {
                var mailbox = new List<long>[size];
                for (int i = 0; i < mailbox.Length; i++)
                {
                    mailbox[i] = new List<long>();
                }
                return mailbox;
            }

#if diagnostics
            int progress = 0;
            const int progressReportSize = 262143;
            var last = (automataCandidates.Length / progressReportSize) * progressReportSize;
            Console.WriteLine("Please wait...");
#endif

            for (int automatonID = 0; automatonID < automataCandidates.Length; automatonID++)
            {
                var temporary = new long[verticesIncludingNew];
                var mailbox = SetupNewMailbox(verticesIncludingNew);
#if diagnostics
                if ((automatonID & progressReportSize) == 0 && automatonID > 0)
                {
                    Interlocked.Add(ref progress, progressReportSize);
                    Console.WriteLine($"Preparing... {(progress) * 100d / automataCandidates.Length}%");
                }
#endif
                generatorHelper.InitializeIteration(automataCandidates[automatonID], individualVerticesHashes[automatonID], mailbox);
                var initialHash = generatorHelper.IterateAndSquashMultiple(automataCandidates[automatonID], individualVerticesHashes[automatonID], temporary, mailbox, estimatedIntegrityLevel);
                insertIdIntoHashbag(hashToGroup, initialHash, automatonID);
            }

            Dictionary<long, SameHashCollisionGroup> integrityLevelConformant = hashToGroup;
            int requiredIntegrityLevel = maximumEstimatedIntegrityLevel;
            while (targetCount > hashToGroup.Count)
            {
#if diagnostics
                Console.WriteLine($"Integrity level is too low: {requiredIntegrityLevel}");
#endif
                requiredIntegrityLevel += 1;
                integrityLevelConformant = new Dictionary<long, SameHashCollisionGroup>();

                while (hashToGroup.Count > 0 && targetCount > hashToGroup.Count)
                {
                    // pass groups that fulfil integrity level requirement
#if diagnostics
                    Console.WriteLine($"LEFTOVER to recompute: {hashToGroup.Count}");
#endif
                    var stack = new Stack<long>(hashToGroup.Keys);
                    while (stack.Count > 0)
                    {
                        var key = stack.Pop();

                        if (targetCount == hashToGroup.Count)
                        {
                            break;
                        }
                        var temporary = new long[verticesIncludingNew];
                        var mailbox = SetupNewMailbox(verticesIncludingNew);
                        
                        var collisionGroup = hashToGroup[key];

                        var queuedHashes = new Queue<long>();
                        var differentHashes = new HashSet<long>();
                        // recompute hashes
                        foreach (var automatonID in collisionGroup.automatonIDs)
                        {
                            var nextHash = generatorHelper.IterateAndSquashMultiple(automataCandidates[automatonID], individualVerticesHashes[automatonID], temporary, mailbox, requiredIntegrityLevel - collisionGroup.agreedIterations);
                            differentHashes.Add(nextHash);
                            queuedHashes.Enqueue(nextHash);
                        }

                        if (differentHashes.Count == 1)
                        {
                            collisionGroup.agreedIterations = requiredIntegrityLevel;
                            hashToGroup.Remove(key, out collisionGroup);
                            integrityLevelConformant.TryAdd(key, collisionGroup);
                        }
                        else
                        {
                            hashToGroup.Remove(key, out collisionGroup);

                            foreach (var automatonID in collisionGroup.automatonIDs)
                                insertIdIntoHashbag(hashToGroup, queuedHashes.Dequeue(), automatonID);

                            foreach (var item in differentHashes)
                                stack.Push(item);
                        }
                    };

                }

                foreach (var kvp in hashToGroup)
                    integrityLevelConformant.TryAdd(kvp.Key, kvp.Value);

                hashToGroup = integrityLevelConformant;
            }

#if diagnostics
            Console.WriteLine($"Integrity level is enough: {requiredIntegrityLevel}");
#endif

            maximumEstimatedIntegrityLevel = requiredIntegrityLevel;

            // return all unique automata
            return integrityLevelConformant.Select(kvp =>
            {
                kvp.Value.automatonIDs.TryPeek(out int representativeAutomatonID);
                return automataCandidates[representativeAutomatonID];
            }).ToArray();

        }


        private static IEnumerable<int[]> GenerateNewMutableClonesFromSmart(int[] transitionFunctions, int newPosition)
        {
            var extendedTransitionFunctions = new int[newPosition + 1];
            transitionFunctions.CopyTo(extendedTransitionFunctions, 0);
            for (int newTransitionTarget = 0, max = newPosition + 1; newTransitionTarget < max; newTransitionTarget += 1)
            {
                extendedTransitionFunctions[newPosition] = newTransitionTarget;

                yield return (int[])extendedTransitionFunctions.Clone();
                
                for (int i = newTransitionTarget; i < newPosition; i += 1)
                {
                    var newTransition = (int[])extendedTransitionFunctions.Clone();
                    newTransition[i] = newPosition;
                    yield return newTransition;
                }
            }
        }
    }
}
