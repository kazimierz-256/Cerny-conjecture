using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public class BinaryAutomata : IBinaryAutomata, IUnaryAutomata, IBinaryAcAutomata, CoreDefinitions.ISolvedOptionalAutomaton
    {
        public byte[] TransitionFunctionsB { get; }

        public byte[] TransitionFunctionsA { get; }

        public List<byte>[] TransitionsFromA { get; }

        private List<byte>[] helpListToGenerate { get; }

        public bool IsVertInAc (int i)
        {
            return TransitionsFromA[i].Count > 0;
        }

        public byte n { get; }

        public byte AcSize { get; }

        public ushort? SynchronizingWordLength { get; }

        private byte minusOne = Byte.MaxValue;

        public BinaryAutomata(int[] tab)
        {
            n = (byte)tab.Length;
            TransitionFunctionsA = new byte[n];
            TransitionFunctionsB = new byte[n];
            TransitionsFromA = new List<byte>[n];
            helpListToGenerate = new List<byte>[n];
            for (byte i = 0; i < n; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }

            for (byte i = 0; i < n; i++)
            {
                TransitionsFromA[tab[i]].Add(i);
                TransitionFunctionsA[i] = (byte)(tab[i]);
            }
            for (byte i = 0; i < n; i++)
            {
                TransitionFunctionsB[i] = IsVertInAc(i) ? (byte)0 : minusOne;
                AcSize += IsVertInAc(i) ? (byte)1 : (byte)0;
            }
        }

        public IEnumerable<IBinaryAcAutomata> MakeAcAutomatas() => AddingBTransition.GenerateAc(this);

        public IEnumerable<IBinaryAutomata> MakeFullAutomatas() => MakingFullAutomata.Generate(this, helpListToGenerate);
    }
}
