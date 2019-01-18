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
            byte[] newBtransition = new byte[optionalAutomata.TransitionFunctionsB.Length];
            Array.Copy(optionalAutomata.TransitionFunctionsB, newBtransition, newBtransition.Length);
            automata = new CoreDefinitions.OptionalAutomaton(optionalAutomata.TransitionFunctionsA, newBtransition);
            
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
