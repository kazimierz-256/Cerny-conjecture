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

        /// <summary>
        /// Be careful! Not entirely sure it is correct
        /// </summary>
        public IEnumerable<CoreDefinitions.IOptionalAutomaton> GenerateAcIncrementally()
        {
            for (int place = 0; place < n; place++)
            {
                if (!isVertInAc[place])
                    automata.TransitionFunctionsB[place] = CoreDefinitions.OptionalAutomaton.MissingTransition;
                else
                {
                    byte i = 0;
                    for (; i < n; i++)
                    {
                        if (isVertInAc[i])
                        {
                            automata.TransitionFunctionsB[place] = i;
                            break;
                        }
                    }

                    // czy to wygląda ok? myślę, że inżyniersko jest równoważne
                    if (i == n)
                        yield break;
                }
            }
            yield return automata;
            var initialBs = (byte[])automata.TransitionFunctionsB.Clone();

            while (true)
            {
                int place = n - 1;
                var foundFollower = false;
                for (; place >= 0; place -= 1)
                {
                    if (!isVertInAc[place])
                    {
                        // cannot go further with this value
                        continue;
                    }
                    else
                    {
                        for (int incremental = automata.TransitionFunctionsB[place] + 1; incremental < n; incremental++)
                        {
                            if (isVertInAc[incremental])
                            {
                                // found ok, following places are 'initial'
                                automata.TransitionFunctionsB[place] = (byte)incremental;
                                foundFollower = true;
                                break;
                            }
                        }

                        if (foundFollower)
                        {
                            break;
                        }
                        else
                        {
                            automata.TransitionFunctionsB[place] = initialBs[place];
                        }
                    }
                }

                if (place < 0)
                    yield break;
                else
                    yield return automata;
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
                automata.TransitionFunctionsB[place] = CoreDefinitions.OptionalAutomaton.MissingTransition;
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