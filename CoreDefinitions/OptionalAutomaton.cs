using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public class OptionalAutomaton : IOptionalAutomaton
    {
        public byte[] TransitionFunctionsB { get; }
        public byte[] TransitionFunctionsA { get; }

        /// <summary>
        /// Requirements:
        /// 1. all unused vertices must have byte.MaxValue as its B- transition function
        /// 2. lengths of arrays of both transition functions A & B must match
        /// </summary>
        public OptionalAutomaton(byte[] TransitionFunctionsA, byte[] TransitionFunctionsB)
        {
            this.TransitionFunctionsA = TransitionFunctionsA;
            this.TransitionFunctionsB = TransitionFunctionsB;
        }

        public int GetMaxCapacitySize() => TransitionFunctionsB.Length;
        public ref readonly byte GetTransitionA(in byte vertex) => ref TransitionFunctionsA[vertex];
        public ref readonly byte GetTransitionB(in byte vertex) => ref TransitionFunctionsB[vertex];
        public bool IsDefinedVertex(in byte vertex) => vertex >= 0 && vertex < TransitionFunctionsB.Length && TransitionFunctionsB[vertex] != byte.MaxValue;

        public ISolvedOptionalAutomaton CreateSolvedObject(bool isSynchronizable, ushort synchronizingWordLength)
            => new SolvedOptionalAutomaton(TransitionFunctionsA, TransitionFunctionsB, isSynchronizable, synchronizingWordLength);

        public override string ToString()
        {
            var builder = new StringBuilder("[[");

            for (byte i = 0; i < GetMaxCapacitySize(); i++)
            {
                if (!IsDefinedVertex(in i)) continue;

                builder.Append(GetTransitionA(i));

                if (i < GetMaxCapacitySize() - 1)
                    builder.Append(",");
            }

            builder.Append("],[");

            for (byte i = 0; i < GetMaxCapacitySize(); i++)
            {
                if (!IsDefinedVertex(in i)) continue;

                builder.Append(GetTransitionB(i));

                if (i < GetMaxCapacitySize() - 1)
                    builder.Append(",");
            }

            builder.Append("]]");
            return builder.ToString();
        }
    }
}
