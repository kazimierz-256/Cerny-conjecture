using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    class MakingFullAutomata
    {
        public static IEnumerable<IBinaryAutomata> Generate(BinaryAutomata a, List<byte>[] helpList)
        {
            for (int i = 0; i < a.n; i++)
            {
                helpList[i] = a.TransitionFunctionsB[i] == Byte.MaxValue ? null : a.TransitionsFromA[a.TransitionFunctionsB[i]];
            }
            return Generate_rec(a, 0, helpList);
        }

        public static IEnumerable<IBinaryAutomata> Generate_rec(BinaryAutomata a, int place, List<byte>[] helpList)
        {
            if (place >= a.n)
            {
                yield return a;
            }
            else if (a.TransitionFunctionsB[place] == Byte.MaxValue)
            {
                for (byte i = 0; i < a.n; i++)
                {
                    a.TransitionFunctionsB[place] = i;
                    foreach (var aut in Generate_rec(a, place + 1, helpList))
                    {
                        yield return aut;
                    }
                }

            }
            else
            {
                foreach (byte b in helpList[place]) //a.TransformFromA[a.TransformB[place]]
                {
                    a.TransitionFunctionsB[place] = b;
                    foreach (var aut in Generate_rec(a, place + 1, helpList))
                    {
                        yield return aut;
                    }
                }

            }
        }
    }
}
