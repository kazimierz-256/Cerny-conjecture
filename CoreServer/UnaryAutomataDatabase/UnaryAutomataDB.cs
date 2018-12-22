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
        public int MinimalLength { get; }
        private readonly HashSet<int> leftoverAutomata
            = new HashSet<int>();

        public string DumpStatistics()
        {
            lock (synchronizingObject)
            {
                return $"Computed {finishedAutomata.Count} out of {leftoverAutomata.Count + processingAutomata.Count + finishedAutomata.Count}. Found interesting {interestingAutomata.Count} automata.";
            }
        }

        public void ProcessInterestingAutomata(List<int> unarySolved, List<List<ISolvedOptionalAutomaton>> solvedInterestingAutomataPerUnary)
        {

        }

        public void MarkAutomatonByIndexAsDone(int automatonIndex)
        {
            processingAutomata.Remove(automatonIndex);
            finishedAutomata.Add(automatonIndex);
        }
        public List<int> GetUnaryAutomataToProcess(int quantity)
        {
            lock (synchronizingObject)
            {
                return leftoverAutomata
                   .Concat(processingAutomata)
                   .Take(quantity)
                   .ToList();
            }
        }

        private readonly HashSet<int> processingAutomata
            = new HashSet<int>();
        private readonly HashSet<int> finishedAutomata
            = new HashSet<int>();

        private readonly Dictionary<int, ISolvedOptionalAutomaton> interestingAutomata = new Dictionary<int, ISolvedOptionalAutomaton>();

        private readonly object synchronizingObject = new object();

        public UnaryAutomataDB(int size)
        {
            Size = size;
            MinimalLength = (size - 1) * (size - 1) / 8;// TODO: tune
            leftoverAutomata = new HashSet<int>(Enumerable.Range(0, theory[size - 1]));
        }
    }
}
