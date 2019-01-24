using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                    description = $"Computed {finishedAutomata.Count(finished => finished != null)} out of {Total}.",
                    finishedAutomata = finishedAutomata.Where(automaton => automaton.solved).ToList(),
                    n = Size
                };
            }
        }

        private int IgnoreThreshold(int n) => (n - 1) * (n - 1);

        public void ProcessInterestingAutomata(ClientServerRequestForMoreAutomata parameters, string userIdentifier, out bool changedMinimum, out bool changedAnything)
        {
            changedMinimum = false;
            var ignoreThreshold = IgnoreThreshold(Size);

            lock (synchronizingObject)
            {
                if (parameters.suggestedMinimumBound > MinimalLength)
                {
                    MinimalLength = parameters.suggestedMinimumBound;
                    changedMinimum = true;
                }

                if (!finishedAutomata[parameters.solution.unaryIndex].solved)
                {
                    var count = 0;

                    for (int j = 0; j < parameters.solution.solvedB.Count; j++)
                    {
                        var syncLength = parameters.solution.solvedSyncLength[j];
                        if (syncLength >= MinimalLength)
                        {
                            if (!synchronizingWordLengthToCount.ContainsKey(syncLength))
                                synchronizingWordLengthToCount.Add(syncLength, 0);

                            synchronizingWordLengthToCount[syncLength] += 1;
                            if (syncLength < ignoreThreshold)
                                count += 1;
                        }
                    }

                    finishedAutomata[parameters.solution.unaryIndex] = new FinishedStatistics()
                    {
                        solution = parameters.solution,
                        clientID = userIdentifier,
                        solved = true
                    };
                    changedAnything = true;

                    AllowedCount += count;
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
                            if (item.solved)
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
                    }
                    #endregion
                }
                else
                {
                    changedAnything = false;
                }
            }
        }

        public List<int> GetAlreadyComputed()
        {
            var finished = new List<int>();
            lock (synchronizingObject)
            {
                foreach (var item in finishedAutomata)
                    if (item.solved)
                        finished.Add(item.solution.unaryIndex);
            }
            return finished;
        }
        private Random randomizer = new Random(0);
        public bool GetUnaryAutomataToProcessAndMarkAsProcessing(out int automatonIndex)
        {
            automatonIndex = -1;

            lock (synchronizingObject)
            {
                while (leftoverAutomata.Count > 0 && automatonIndex == -1)
                {
                    var dequeued = leftoverAutomata.Dequeue();
                    if (!finishedAutomata[dequeued].solved)
                        automatonIndex = dequeued;
                }
                if (automatonIndex != -1)
                {
                    leftoverAutomata.Enqueue(automatonIndex);
                }
            }
            return automatonIndex != -1;
        }

        public UnaryAutomataDB(int size, int maximumLongestAutomataCount)
        {
            MaximumLongestAutomataCount = maximumLongestAutomataCount;
            Size = size;
            MinimalLength = (size - 1) * (size - 1) * 3 / 4;
            AllowedCount = 0;

            Total = theory[size - 1];
            var automataIndices = new int[Total];
            finishedAutomata = new FinishedStatistics[Total];
            var random = new Random(1);
            var automataValues = new double[Total];
            for (int i = 0; i < Total; i += 1)
            {
                automataIndices[i] = i;
                automataValues[i] = random.NextDouble();
                finishedAutomata[i] = new FinishedStatistics();
            }

            Array.Sort(automataValues, automataIndices);

            for (int i = 0; i < Total; i += 1)
                leftoverAutomata.Enqueue(automataIndices[i]);
        }

        public int Size { get; private set; }
        public int MinimalLength { get; private set; }
        public int Total { get; private set; }
        public int Found => finishedAutomata.Count(automaton => automaton.solved);

        public int MaximumLongestAutomataCount = 0;
        private int AllowedCount;

        private readonly Queue<int> leftoverAutomata = new Queue<int>();
        private FinishedStatistics[] finishedAutomata = new FinishedStatistics[0];
        private readonly SortedDictionary<int, int> synchronizingWordLengthToCount = new SortedDictionary<int, int>();

        private readonly object synchronizingObject = new object();

        public DBSerialized ExportAsXMLString()
        {
            lock (synchronizingObject)
            {
                return new DBSerialized()
                {
                    finishedStatistics = finishedAutomata,
                    Size = Size,
                    MaximumLongestAutomataCount = MaximumLongestAutomataCount,
                    AllowedCount = AllowedCount
                };
            }
        }

        public void ImportShallow(DBSerialized data)
        {
            Console.WriteLine("Importing database from file...");
            var leftoverAutomataIndices = new HashSet<int>(Enumerable.Range(0, theory[Size - 1]));
            lock (synchronizingObject)
            {
                Size = data.Size;
                Total = theory[Size - 1];
                MaximumLongestAutomataCount = data.MaximumLongestAutomataCount;
                AllowedCount = data.AllowedCount;

                leftoverAutomata.Clear();
                finishedAutomata = data.finishedStatistics;
                synchronizingWordLengthToCount.Clear();
                foreach (var item in data.finishedStatistics)
                {
                    if (item.solved)
                    {
                        leftoverAutomataIndices.Remove(item.solution.unaryIndex);
                        foreach (var automatonLength in item.solution.solvedSyncLength)
                        {
                            if (!synchronizingWordLengthToCount.ContainsKey(automatonLength))
                                synchronizingWordLengthToCount.Add(automatonLength, 0);
                            synchronizingWordLengthToCount[automatonLength] += 1;
                        }
                    }
                }
                var automataIndices = new int[leftoverAutomataIndices.Count];
                leftoverAutomataIndices.CopyTo(automataIndices);
                var random = new Random(1);
                var automataValues = new double[automataIndices.Length];
                for (int i = 0; i < automataIndices.Length; i += 1)
                    automataValues[i] = random.NextDouble();

                Array.Sort(automataValues, automataIndices);
                foreach (var item in automataIndices)
                {
                    leftoverAutomata.Enqueue(item);
                }
            }

        }
    }
}
