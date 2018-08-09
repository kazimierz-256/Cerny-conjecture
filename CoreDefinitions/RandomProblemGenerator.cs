using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomataIteratorTests
{
    public static class RandomProblemGenerator
    {
        public static IEnumerable<IOptionalAutomaton> Generate(int size, int seed)
        {
            var a = new byte[size];
            var b = new byte[size];
            var automatonToReturn = new OptionalAutomaton(a, b);
            var random = new Random(seed);

            while (true)
            {
                for (int i = 0; i < size; i++)
                {
                    a[i] = (byte)(random.Next() % size);
                    b[i] = (byte)(random.Next() % size);
                }
                yield return automatonToReturn;
            }
        }
    }
}