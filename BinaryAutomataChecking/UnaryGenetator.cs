using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public class UnaryGenetator
    {
        static public IEnumerable<int[]> Generate(int n)
        {
            UniqueUnaryAutomata.Generator endofunctorsGenerator = new UniqueUnaryAutomata.Generator();
            IEnumerator<int[][]> enumerator = endofunctorsGenerator.GetAllUniqueAutomataOfSize().GetEnumerator();
            enumerator.MoveNext();
            int[][] endofunctors = enumerator.Current;
            while (endofunctors[0].Length < n)
            {
                enumerator.MoveNext();
                endofunctors = enumerator.Current;
            }
            foreach (var endo in endofunctors)
            {
                //może wszystko z jednego, tylko zmieniać tablicę a?
                yield return endo;
            }
            
        }
    }
}
