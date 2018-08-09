using System;
using System.Collections.Generic;
using System.Text;

namespace UniqueUnaryAutomata
{
    public class GeneratorHelper
    {
        const long hashConstant = 0b101000001000001000101000100000101000001000101000101000101011;

        private static long[] precomputedArray;

        public void PrepareFor(int uppperBound)
        {
            precomputedArray = new long[uppperBound + 2];
            precomputedArray[0] = SquashOrdered(hashConstant, hashConstant);

            for (int i = 1; i < precomputedArray.Length; i++)
                precomputedArray[i] = SquashOrdered(precomputedArray[i - 1], hashConstant);

        }

        private long SquashOrdered(long initialValue, IEnumerable<long> orderedList)
        {
            foreach (var itemHash in orderedList)
            {
                initialValue |= 1;
                initialValue *= hashConstant;
                initialValue += itemHash;
            }
            return initialValue;
        }

        private long SquashOrdered(long initialValue, params long[] orderedList)
        {
            foreach (var itemHash in orderedList)
            {
                initialValue |= 1;
                initialValue *= hashConstant;
                initialValue += itemHash;
            }
            return initialValue;
        }

        private long SquashOrdered(long initualValue, long append) => (initualValue |= 1) * hashConstant + (append ^ hashConstant);

        public void InitializeIteration(int[] transitionFunctions, long[] hashes, List<long>[] mailbox)
        {
            for (int i = 0; i < transitionFunctions.Length; i += 1)
                mailbox[i] = new List<long>();

            var assigned = new int[transitionFunctions.Length];
            var distances = new int[transitionFunctions.Length];
            var visitedDistancePlus1 = new int[transitionFunctions.Length];

            for (int i = 0; i < transitionFunctions.Length; i++)
            {
                if (assigned[i] > 0)
                    continue;

                var loopingIndex = -1;
                var loopingDistance = -1;
                int considering = i;
                int iteration0 = transitionFunctions.Length * (i + 1);

                for (int iteration = iteration0 + 1, max = iteration0 + transitionFunctions.Length + 2; iteration < max; iteration += 1)
                {
                    if (assigned[considering] > 0 && assigned[considering] != iteration0)
                    {
                        // already computed vertex but in some other iteration
                        break;
                    }
                    else
                    {
                        if (assigned[considering] == iteration0)
                        {// already computed in this iteration
                            loopingIndex = considering;
                            loopingDistance = iteration - visitedDistancePlus1[considering];

                            //FINISH
                            for (int j = 0; j < loopingDistance; j++, considering = transitionFunctions[considering])
                            {
                                distances[considering] = loopingDistance;
                            }

                            break;
                        }
                        else
                        {// not yet computed in this iteration
                            assigned[considering] = iteration0;
                            visitedDistancePlus1[considering] = iteration;
                            considering = transitionFunctions[considering];
                        }
                    }
                }
            }

            for (int i = 0; i < transitionFunctions.Length; i++)
            {
                hashes[i] = precomputedArray[distances[i]];
            }
        }

        public long IterateAndSquashMultiple(int[] transitionFunctions, long[] hashes, long[] temporary, List<long>[] mailbox, int replyBacks)
        {
            int i;

            for (int reply = 0; reply < replyBacks; reply++)
            {
                for (i = 0; i < transitionFunctions.Length; i += 1)
                {
                    mailbox[transitionFunctions[i]].Add(hashes[i]);
                }

                // read mailbox hashes, sort them, and SQUASH them together with current hash
                for (i = 0; i < transitionFunctions.Length; i += 1)
                {
                    mailbox[i].Sort();
                    hashes[i] = SquashOrdered(hashes[i], mailbox[i]);
                }

                // reply back the hashes 
                for (i = 0; i < transitionFunctions.Length; i += 1)
                    mailbox[i].Add(SquashOrdered(
                        hashes[i],
                        hashes[transitionFunctions[transitionFunctions[i]]]
                        ));

                for (i = 0; i < transitionFunctions.Length; i += 1)
                {
                    hashes[i] = mailbox[i][mailbox[i].Count - 1];
                    mailbox[i].Clear();
                }
            }

            hashes.CopyTo(temporary, 0);

            Array.Sort(temporary);
            return SquashOrdered(0L, temporary);
        }

    }
}
