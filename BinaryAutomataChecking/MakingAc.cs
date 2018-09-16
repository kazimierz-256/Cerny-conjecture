using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public class AddingBTransition
    {
        public static IEnumerable<IBinaryAcAutomata> GenerateAc(BinaryAutomata unaryAtomata) => Generate_rec(0, unaryAtomata);

        public static IEnumerable<IBinaryAcAutomata> Generate_rec(int place, BinaryAutomata automata)
        {
            if (place >= automata.n)
            {
                yield return automata;
            }
            else if (!automata.IsVertInAc(place))
            {
                foreach(var a in Generate_rec(place + 1, automata))
                {
                    yield return a;
                }
            }
            else
            {
                for (byte i = 0; i < automata.n; i++)
                {
                    if (automata.IsVertInAc(i))
                    {
                        automata.TransitionFunctionsB[place] = i;
                        foreach (var a in Generate_rec(place + 1,automata))
                        {
                            yield return a;
                        }
                    }

                }
            }
            
        }


    }
}
