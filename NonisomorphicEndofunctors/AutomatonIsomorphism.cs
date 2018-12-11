using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniqueUnaryAutomata
{
    public static class AutomatonIsomorphism
    {
        public static bool AreAutomataIsomorphic(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;

            var mapAtoB = new Dictionary<int, int>();
            var mapBtoA = new Dictionary<int, int>();

            var bAlreadyFirstMapped = new HashSet<int>();
            for (int bCandidate = 0; bCandidate < b.Length; bCandidate++)
            {
                if (bAlreadyFirstMapped.Contains(bCandidate))
                    continue;
                if (ConsiderMatching(0, bCandidate, a, b, mapAtoB, mapBtoA))
                    return true;

                bAlreadyFirstMapped.Add(bCandidate);
            }
            return false;
        }

        private static bool ConsiderMatching(int aCandidate, int bCandidate, int[] a, int[] b, Dictionary<int, int> mapAtoB, Dictionary<int, int> mapBtoA)
        {
            mapAtoB.Add(aCandidate, bCandidate);
            mapBtoA.Add(bCandidate, aCandidate);
            // verify matching is correct
            var locallyIsomorphic = true;
            foreach (var aMiniMap in mapAtoB)
            {
                var mapping = a[aMiniMap.Key];
                if (mapAtoB.ContainsKey(mapping))
                {
                    if (mapAtoB[mapping] != b[aMiniMap.Value])
                    {
                        locallyIsomorphic = false;
                        break;
                    }
                }
            }

            var isomorphic = locallyIsomorphic && mapAtoB.Count == a.Length;
            if (locallyIsomorphic && !isomorphic)
            {
                var aFurtherCandidate = mapAtoB.Count;

                for (int bFurtherCandidate = 0; bFurtherCandidate < b.Length; bFurtherCandidate += 1)
                {
                    if (!mapBtoA.ContainsKey(bFurtherCandidate) && ConsiderMatching(aFurtherCandidate, bFurtherCandidate, a, b, mapAtoB, mapBtoA))
                    {
                        isomorphic = true;
                        break;
                    }
                }
            }

            mapAtoB.Remove(aCandidate);
            mapBtoA.Remove(bCandidate);
            return isomorphic;
        }
    }
}
