using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.UnaryAutomataDatabase
{
    public class UnaryAutomataDB
    {
        private readonly static int[] theory = new int[]
        {
            1, 3, 7, 19, 47, 130, 343, 951, 2615, 7318, 20491, 57903, 163898, 466199, 1328993, 3799624, 10884049, 31241170
        };
        private readonly HashSet<int> leftoverAutomata
            = new HashSet<int>();
        private readonly HashSet<int> processingAutomata
            = new HashSet<int>();
        private readonly HashSet<int> finishedAutomata
            = new HashSet<int>();

        public UnaryAutomataDB(int size)
        {
            leftoverAutomata =
                new HashSet<int>(Enumerable.Range(0, theory[size - 1]));
        }
        public int[] GetUnaryAutomataIndices(int count)
            => leftoverAutomata
                .Concat(processingAutomata)
                .Take(count)
                .ToArray();

        public void MarkAutomatonAsDone(int automatonIndex)
        {
            processingAutomata.Remove(automatonIndex);
            finishedAutomata.Add(automatonIndex);
        }
    }
}
