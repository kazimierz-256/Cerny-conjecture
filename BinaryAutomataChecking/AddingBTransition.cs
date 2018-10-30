using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public class AddingBTransition
    {
        CoreDefinitions.IOptionalAutomaton automata;
        int n;
        bool[] isVertInAc;

        public AddingBTransition(CoreDefinitions.IOptionalAutomaton optionalAutomaton, bool[] isVertInAcTab)
        {
            automata = optionalAutomaton;
            n = optionalAutomaton.TransitionFunctionsA.Length;
            isVertInAc = new bool[n];
            for (int i = 0; i < n; i++)
            {
                isVertInAc[automata.TransitionFunctionsA[i]] = true;
            }
        }

        public IEnumerable<CoreDefinitions.IOptionalAutomaton> GenerateAc() => Generate_rec(0);

        private IEnumerable<CoreDefinitions.IOptionalAutomaton> Generate_rec(int place)
        {
            if (place >= n)
            {
                yield return automata;
            }
            else if (!isVertInAc[place])
            {
                automata.TransitionFunctionsB[place] = Byte.MaxValue;
                foreach (var a in Generate_rec(place + 1))
                {
                    yield return a;
                }
            }
            else
            {
                for (byte i = 0; i < n; i++)
                {
                    if (isVertInAc[i])
                    {
                        automata.TransitionFunctionsB[place] = i;
                        foreach (var a in Generate_rec(place + 1))
                        {
                            yield return a;
                        }
                    }
                }
            }
        }

        public static int MakeIsVertInAcTabAndGetAcSize(int[] unaryAutomata, out bool[] isVertInAc)
        {
            int size = unaryAutomata.Length;
            isVertInAc = new bool[size];
            for (int i = 0; i < size; i++)
            {
                isVertInAc[unaryAutomata[i]] = true;
            }
            int AcSize = 0;
            for (int i = 0; i < size; i++)
            {
                AcSize += isVertInAc[i] ? 1 : 0;
            }
            return AcSize;
        }

        public static int MakeIsVertInAcTabAndGetAcSize(byte[] unaryAutomata, out bool[] isVertInAc)
        {
            int size = unaryAutomata.Length;
            isVertInAc = new bool[size];
            for (int i = 0; i < size; i++)
            {
                isVertInAc[unaryAutomata[i]] = true;
            }
            int AcSize = 0;
            for (int i = 0; i < size; i++)
            {
                AcSize += isVertInAc[i] ? 1 : 0;
            }
            return AcSize;
        }
    }
}