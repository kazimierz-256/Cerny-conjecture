using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataChecking
{
    public class MakingFullAutomata
    {
        private int n;
        public List<byte>[] TransitionsFromA;
        private List<byte>[] helpList;
        private CoreDefinitions.IOptionalAutomaton automata;

        public MakingFullAutomata(CoreDefinitions.IOptionalAutomaton optionalAutomata)
        {
            automata = optionalAutomata;
            n = optionalAutomata.TransitionFunctionsA.Length;
            //List<byte>[] 
            TransitionsFromA = new List<byte>[n];
            for (byte i = 0; i < n; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }
            for (byte i = 0; i < n; i++)
            {
                TransitionsFromA[automata.TransitionFunctionsA[i]].Add(i);
            }

            helpList = new List<byte>[n];
            for (int i = 0; i < n; i++)
            {
                helpList[i] = automata.TransitionFunctionsB[i] == CoreDefinitions.OptionalAutomaton.MissingTransition ? null : TransitionsFromA[automata.TransitionFunctionsB[i]];
            }
        }

        /// <summary>
        /// Be careful! Not entirely sure it is correct
        /// </summary>
        public IEnumerable<CoreDefinitions.IOptionalAutomaton> GenerateIncrementally()
        {
            for (int place = 0; place < n; place++)
            {
                if (helpList[place] == null)
                    automata.TransitionFunctionsB[place] = 0;
                else
                {
                    if (helpList[place].Count == 0)
                        yield break;

                    automata.TransitionFunctionsB[place] = helpList[place][0];
                }
            }

            yield return automata;
            var initialBs = (byte[])automata.TransitionFunctionsB.Clone();
            var helpListIter = new int[n];

            while (true)
            {
                int place = n - 1;
                for (; place >= 0; place -= 1)
                {
                    if (helpList[place] == null)
                    {
                        if (automata.TransitionFunctionsB[place] + 1 == n)
                        {
                            automata.TransitionFunctionsB[place] = initialBs[place];
                            continue;
                        }
                        else
                        {
                            automata.TransitionFunctionsB[place] += 1;
                            break;
                        }
                    }
                    else
                    {
                        if (helpListIter[place] + 1 == helpList[place].Count)
                            helpListIter[place] = 0;
                        else
                            helpListIter[place] += 1;

                        automata.TransitionFunctionsB[place] = helpList[place][helpListIter[place]];
                    }
                }

                if (place < 0)
                    yield break;
                else
                    yield return automata;
            }
        }

        public IEnumerable<CoreDefinitions.IOptionalAutomaton> Generate()
        {
            return Generate_rec(0);
        }

        private IEnumerable<CoreDefinitions.IOptionalAutomaton> Generate_rec(int place)
        {
            if (place >= n)
            {
                yield return automata;
            }
            else if (helpList[place] == null)//(automata.TransitionFunctionsB[place] == CoreDefinitions.OptionalAutomaton.MissingTransition)//
            {
                for (byte i = 0; i < n; i++)
                {
                    automata.TransitionFunctionsB[place] = i;
                    foreach (var aut in Generate_rec(place + 1))
                    {
                        yield return aut;
                    }
                }
            }
            else
            {
                foreach (byte b in helpList[place])//TransitionsFromA[automata.TransitionFunctionsB[place]])
                {
                    automata.TransitionFunctionsB[place] = b;
                    foreach (var aut in Generate_rec(place + 1))
                    {
                        yield return aut;
                    }
                }
            }
        }
    }
}
