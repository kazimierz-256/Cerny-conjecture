using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreDefinitions;

namespace CoreServer.UnaryAutomataDatabase
{
    public class UnaryAutomataDB
    {
        private readonly static int[] theory = new int[]
        {
            1, 3, 7, 19, 47, 130, 343, 951, 2615, 7318, 20491, 57903, 163898, 466199, 1328993, 3799624, 10884049, 31241170
        };

        public int Size { get; }
        public int MinimalLength { get; private set; }
        public int Total { get; }
        public const int MaximumLongestAutomataCount = 100;
        private int allowedCount = 0;

        private readonly Queue<int> leftoverAutomata = new Queue<int>();

        public string DumpStatistics()
        {
            lock (synchronizingObject)
            {
                return $"Computed {finishedAutomata.Count} out of {Total}. Found interesting {interestingAutomataIndexToSolved.Count} automata.";
            }
        }

        public void ProcessInterestingAutomata(List<int> unarySolved, List<byte[]> toSendSolvedUnary, List<List<ushort>> toSendSolvedSyncLength, List<List<byte[]>> toSendSolvedB, out bool changedMinimum)
        {
            changedMinimum = false;
            if (unarySolved.Count != toSendSolvedUnary.Count || unarySolved.Count != toSendSolvedSyncLength.Count || unarySolved.Count != toSendSolvedB.Count)
                return;

            lock (synchronizingObject)
            {
                for (int i = 0; i < unarySolved.Count; i += 1)
                {
                    if (!interestingAutomataIndexToSolved.ContainsKey(unarySolved[i]))
                    {
                        var list = new List<ISolvedOptionalAutomaton>();
                        // update sync word length stats

                        for (int j = 0; j < toSendSolvedB[i].Count; j++)
                        {
                            var syncLength = toSendSolvedSyncLength[i][j];
                            if (syncLength >= MinimalLength)
                            {
                                if (!synchronizingWordLengthToCount.ContainsKey(syncLength))
                                    synchronizingWordLengthToCount.Add(syncLength, 0);

                                synchronizingWordLengthToCount[syncLength] += 1;
                                list.Add(new SolvedOptionalAutomaton((byte[])toSendSolvedUnary[i].Clone(), toSendSolvedB[i][j], syncLength));
                            }
                        }
                        interestingAutomataIndexToSolved.Add(unarySolved[i], list);
                        finishedAutomata.Enqueue(new FinishedStatistics() { unaryAutomatonIndex = unarySolved[i], finishedTime = DateTime.Now });
                        allowedCount += list.Count;
                    }
                }


                #region Update minimum bound
                var leftover = allowedCount;
                var removeUpTo = -1;
                foreach (var item in synchronizingWordLengthToCount)
                {
                    if (leftover > MaximumLongestAutomataCount)
                    {
                        leftover -= item.Value;
                        removeUpTo = item.Key;
                    }
                    else
                    {
                        break;
                    }
                }

                if (removeUpTo + 1 > MinimalLength)
                {
                    MinimalLength = removeUpTo + 1;
                    changedMinimum = true;
                }
                #endregion

            }
        }

        public List<int> GetUnaryAutomataToProcessAndMarkAsProcessing(int quantity)
        {
            var toProcess = new List<int>();
            lock (synchronizingObject)
            {

                while (leftoverAutomata.Count > 0 && toProcess.Count < quantity)
                    toProcess.Add(leftoverAutomata.Dequeue());

                while (processingOrFinishedAutomata.Count > 0 && toProcess.Count < quantity)
                {
                    var dequeued = processingOrFinishedAutomata.Dequeue();
                    if (interestingAutomataIndexToSolved.ContainsKey(dequeued))
                    {
                        // discard the item, it is already computed (in the finished queue)
                    }
                    else
                    {
                        toProcess.Add(dequeued);
                    }
                }

                foreach (var item in toProcess)
                    processingOrFinishedAutomata.Enqueue(item);
            }
            return toProcess;
        }
        public class FinishedStatistics
        {
            public int unaryAutomatonIndex;
            public DateTime finishedTime;
        }
        // may contain already computed!
        private readonly Queue<int> processingOrFinishedAutomata = new Queue<int>();
        private readonly Queue<FinishedStatistics> finishedAutomata = new Queue<FinishedStatistics>();

        private readonly SortedDictionary<int, int> synchronizingWordLengthToCount = new SortedDictionary<int, int>();
        private readonly Dictionary<int, List<ISolvedOptionalAutomaton>> interestingAutomataIndexToSolved = new Dictionary<int, List<ISolvedOptionalAutomaton>>();

        private readonly object synchronizingObject = new object();

        public UnaryAutomataDB(int size)
        {
            Size = size;
            MinimalLength = (size - 1) * (size - 1) / 3;

            Total = theory[size - 1];
            var automataIndices = new int[Total];
            var random = new Random(1);
            var automataValues = new double[Total];
            for (int i = 0; i < Total; i += 1)
            {
                automataIndices[i] = i;
                automataValues[i] = random.NextDouble();
            }

            Array.Sort(automataValues, automataIndices);

            for (int i = 0; i < Total; i += 1)
                leftoverAutomata.Enqueue(automataIndices[i]);
        }
    }
}
