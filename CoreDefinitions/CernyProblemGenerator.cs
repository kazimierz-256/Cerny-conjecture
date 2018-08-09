using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomataIteratorTests
{
    public static class CernyProblemGenerator
    {
        public static IEnumerable<IOptionalAutomaton> Generate(int startingFrom = 3)
        {
            for (int n = Math.Max(3, startingFrom); n < byte.MaxValue; n++)
            {
                var a = Enumerable.Range(0, n)
                .Select(i => i == 0 ? (byte)1 : (byte)i);

                var b = Enumerable.Range(0, n)
                .Select(i => (i + 1) == n ? (byte)0 : (byte)(i + 1));

                // tak nie należy używać :) lepiej zajżyj do randomGenerator, tutaj inaczej się nie za bardzo da
                yield return new OptionalAutomaton(a.ToArray(), b.ToArray());
            }
        }
    }
}
