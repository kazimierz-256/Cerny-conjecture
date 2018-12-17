using System;
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
        private readonly HashSet<int> leftoverAutomata
            = new HashSet<int>();

        public string DumpStatistics()
        {
            lock (synchronizingObject)
            {
                return $"Computed {finishedAutomata.Count} out of {leftoverAutomata.Count + processingAutomata.Count + finishedAutomata.Count}. Found interesting {interestingAutomata.Count} automata.";
            }
        }

        private readonly HashSet<int> processingAutomata
            = new HashSet<int>();
        private readonly HashSet<int> finishedAutomata
            = new HashSet<int>();
        private readonly HashSet<ISolvedOptionalAutomaton> interestingAutomata = new HashSet<ISolvedOptionalAutomaton>();

        public Dictionary<long, int[]> idsToAutomata = new Dictionary<long, int[]>();
        private Random random = new Random(0);
        private object synchronizingObject = new object();
        private long GenerateNewPacketID()
        {
            long newPacketID = 0L;
            do
            {
                newPacketID = (long)(random.Next() << 32) + random.Next();
            } while (idsToAutomata.ContainsKey(newPacketID));
            return newPacketID;
        }

        public void AddInterestingAutomaton(ISolvedOptionalAutomaton solvedInterestingAutomaton)
        {
            lock (synchronizingObject)
            {
                if (!interestingAutomata.Contains(solvedInterestingAutomaton))
                    interestingAutomata.Add(solvedInterestingAutomaton);
            }
        }

        public UnaryAutomataDB(int size)
        {
            Size = size;
            leftoverAutomata =
               new HashSet<int>(Enumerable.Range(0, theory[size - 1]));
        }
        private int[] GetUnaryAutomataIndices(int count)
            => leftoverAutomata
                .Concat(processingAutomata)
                .Take(count)
                .ToArray();

        public void GenerateNewPacket(int limitedQuantity, out long newPacketID, out int[] automataPacket)
        {
            lock (synchronizingObject)
            {
                newPacketID = GenerateNewPacketID();
                automataPacket = GetUnaryAutomataIndices(limitedQuantity);
                foreach (var automatonIndex in automataPacket)
                {
                    leftoverAutomata.Remove(automatonIndex);
                    processingAutomata.Add(automatonIndex);
                }
                idsToAutomata.Add(newPacketID, automataPacket);
            }
        }

        private void MarkAutomatonByIndexAsDone(int automatonIndex)
        {
            processingAutomata.Remove(automatonIndex);
            finishedAutomata.Add(automatonIndex);
        }

        public bool MarkAsSolvedAutomata(long id)
        {
            lock (synchronizingObject)
            {
                if (idsToAutomata.ContainsKey(id))
                {
                    foreach (var automatonIndex in idsToAutomata[id])
                        MarkAutomatonByIndexAsDone(automatonIndex);

                    idsToAutomata.Remove(id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
