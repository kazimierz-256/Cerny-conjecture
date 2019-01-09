using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunicationContracts;
using CoreDefinitions;

namespace CoreServer.UnaryAutomataDatabase
{
    public class UnaryAutomataDB
    {
        private readonly static int[] theory = new int[]
        {
            1, 3, 7, 19, 47, 130, 343, 951, 2615, 7318, 20491, 57903, 163898, 466199, 1328993, 3799624, 10884049, 31241170
        };
        public ServerPresentationComputationSummary DumpStatistics()
        {
            lock (synchronizingObject)
            {
                return new ServerPresentationComputationSummary()
                {
                    total = Total,
                    description = $"Computed {finishedAutomata.Count} out of {Total}.",
                    finishedAutomata = finishedAutomata.ToList()
                };
            }
        }

        public void ProcessInterestingAutomata(ClientServerRequestForMoreAutomata parameters, out bool changedMinimum, string userIdentifier)
        {
            changedMinimum = false;

            lock (synchronizingObject)
            {
                if (parameters.suggestedMinimumBound > MinimalLength)
                {
                    MinimalLength = parameters.suggestedMinimumBound;
                    changedMinimum = true;
                }
                var finishTime = DateTime.Now;
                for (int i = 0; i < parameters.solutions.Count; i += 1)
                {
                    if (!solvedAutomataIndices.Contains(parameters.solutions[i].unaryIndex))
                    {
                        //var list = new List<ISolvedOptionalAutomaton>();
                        var count = 0;
                        // update sync word length stats

                        for (int j = 0; j < parameters.solutions[i].solvedB.Count; j++)
                        {
                            var syncLength = parameters.solutions[i].solvedSyncLength[j];
                            if (syncLength >= MinimalLength)
                            {
                                if (!synchronizingWordLengthToCount.ContainsKey(syncLength))
                                    synchronizingWordLengthToCount.Add(syncLength, 0);

                                synchronizingWordLengthToCount[syncLength] += 1;
                                count += 1;
                            }
                        }
                        solvedAutomataIndices.Add(parameters.solutions[i].unaryIndex);//, list);
                        finishedAutomata.Enqueue(new FinishedStatistics()
                        {
                            solution = parameters.solutions[i],
                            finishTime = finishTime,
                            issueTime = issueTime[parameters.solutions[i].unaryIndex],
                            clientID = userIdentifier
                        });
                        AllowedCount += count;
                    }
                }


                #region Update minimum bound
                if (AllowedCount > MaximumLongestAutomataCount)
                {
                    var removeUpTo = -1;
                    var toDeleteWordLength = new List<int>();
                    foreach (var item in synchronizingWordLengthToCount)
                    {
                        if (AllowedCount > MaximumLongestAutomataCount)
                        {
                            AllowedCount -= item.Value;
                            removeUpTo = item.Key;
                            toDeleteWordLength.Add(item.Key);
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

                    foreach (var item in toDeleteWordLength)
                        synchronizingWordLengthToCount.Remove(item);

                    foreach (var item in finishedAutomata)
                    {
                        var leftoverSyncLengths = new List<ushort>();
                        var leftoverBAutomata = new List<byte[]>();
                        for (int i = 0; i < item.solution.solvedB.Count; i++)
                        {
                            if (item.solution.solvedSyncLength[i] >= MinimalLength)
                            {
                                leftoverSyncLengths.Add(item.solution.solvedSyncLength[i]);
                                leftoverBAutomata.Add(item.solution.solvedB[i]);
                            }
                        }
                        item.solution.solvedSyncLength = leftoverSyncLengths;
                        item.solution.solvedB = leftoverBAutomata;

                    }
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
                    if (solvedAutomataIndices.Contains(dequeued))
                    {
                        // discard the item, it is already computed (in the finished queue)
                    }
                    else
                    {
                        toProcess.Add(dequeued);
                    }
                }

                var time = DateTime.Now;
                foreach (var index in toProcess)
                {
                    processingOrFinishedAutomata.Enqueue(index);
                    if (!issueTime.ContainsKey(index))
                        issueTime.Add(index, time);
                }
            }
            return toProcess;
        }

        public UnaryAutomataDB(int size, int maximumLongestAutomataCount)
        {
            MaximumLongestAutomataCount = maximumLongestAutomataCount;
            Size = size;
            MinimalLength = (size - 1) * (size - 1) / 8;
            AllowedCount = 0;

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

        public int Size { get; private set; }
        public int MinimalLength { get; private set; }
        public int Total { get; private set; }
        public int MaximumLongestAutomataCount = 0;
        private int AllowedCount;

        private readonly Queue<int> leftoverAutomata = new Queue<int>();

        private readonly Dictionary<int, DateTime> issueTime = new Dictionary<int, DateTime>();
        // may contain already computed!
        private readonly Queue<int> processingOrFinishedAutomata = new Queue<int>();
        private readonly Queue<FinishedStatistics> finishedAutomata = new Queue<FinishedStatistics>();

        private readonly SortedDictionary<int, int> synchronizingWordLengthToCount = new SortedDictionary<int, int>();
        private readonly HashSet<int> solvedAutomataIndices = new HashSet<int>();

        private readonly object synchronizingObject = new object();

        public ProgressIO.ProgressIO Export()
        {
            lock (synchronizingObject)
            {
                return new ProgressIO.ProgressIO()
                {
                    finishedStatistics = finishedAutomata.ToList(),
                    Size = Size,
                    MaximumLongestAutomataCount = MaximumLongestAutomataCount,
                    AllowedCount = AllowedCount
                };
            }
        }

        public void ImportShallow(ProgressIO.ProgressIO data)
        {
            Console.WriteLine("importing database from file...");
            var leftoverAutomataIndices = new HashSet<int>(Enumerable.Range(0, theory[Size - 1]));
            lock (synchronizingObject)
            {
                Size = data.Size;
                Total = theory[Size - 1];
                MaximumLongestAutomataCount = data.MaximumLongestAutomataCount;
                AllowedCount = data.AllowedCount;

                leftoverAutomata.Clear();
                issueTime.Clear();
                processingOrFinishedAutomata.Clear();
                finishedAutomata.Clear();
                synchronizingWordLengthToCount.Clear();
                solvedAutomataIndices.Clear();
                foreach (var item in data.finishedStatistics)
                {
                    leftoverAutomataIndices.Remove(item.solution.unaryIndex);
                    finishedAutomata.Enqueue(item);
                    solvedAutomataIndices.Add(item.solution.unaryIndex);
                    foreach (var automatonLength in item.solution.solvedSyncLength)
                    {
                        if (!synchronizingWordLengthToCount.ContainsKey(automatonLength))
                            synchronizingWordLengthToCount.Add(automatonLength, 0);
                        synchronizingWordLengthToCount[automatonLength] += 1;
                    }
                }
            }
            foreach (var item in leftoverAutomataIndices)
            {
                leftoverAutomata.Enqueue(item);
            }
        }
    }
}
