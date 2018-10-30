using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryAutomataChecking
{
    static public class BinaryAutomataIterator
    {
        public static IEnumerable<CoreDefinitions.ISolvedOptionalAutomaton> GetAllWithLongSynchronizedWord(int minimalLenght, int size)
        {
            AutomataIterator.ISolutionMapperReusable solutionMapperReusable;
            if (size <= 12)
            {
                solutionMapperReusable = new AutomataIterator.PowerAutomatonReusableSolutionMapperFastMaximum12();
            }
            else if (size <= 16)
            {
                solutionMapperReusable = new AutomataIterator.PowerAutomatonReusableSolutionMapperFastMaximum16();
            }
            else
            {
                throw new ArgumentException();
            }
            return
                from automata in solutionMapperReusable.SelectAsSolved(GetAllFullAutomatasToCheck(size))
                where automata.SynchronizingWordLength != null
                where automata.SynchronizingWordLength > minimalLenght
                select automata;
        }

        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllFullAutomatasToCheck(int size)
        {
            AutomataIterator.ISolutionMapperReusable solutionMapperReusable;
            if (size <= 12)
            {
                solutionMapperReusable = new AutomataIterator.PowerAutomatonReusableSolutionMapperFastMaximum12();
            }
            else if (size <= 16)
            {
                solutionMapperReusable = new AutomataIterator.PowerAutomatonReusableSolutionMapperFastMaximum16();
            }
            else
            {
                throw new ArgumentException();
            }
            int maxLenght = (size - 1) * (size - 1);
            IEnumerable<CoreDefinitions.IOptionalAutomaton> AcAutomatas = 
                from automata in solutionMapperReusable.SelectAsSolved(GetAllAcAutomatasToCheck(size))
                where automata.SynchronizingWordLength==null || 2 * automata.SynchronizingWordLength + 1 > maxLenght
                select automata;

            foreach (var AcAutomat in AcAutomatas)
            {
                MakingFullAutomata makingFullAutomata = new MakingFullAutomata(AcAutomat);
                foreach(var fullAutomat in makingFullAutomata.Generate())
                {
                    yield return fullAutomat;
                }
            }
        }

        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllAcAutomatasToCheck (int size)
        {
            byte[] TranA = new byte[size], TranB = new byte[size];
            CoreDefinitions.IOptionalAutomaton unaryAutomata = new CoreDefinitions.OptionalAutomaton(TranA, TranB);
            foreach (int[] endoFunctor in UnaryGenetator.Generate(size))
            {
                bool[] isVertInAcTab;
                int AcSize = AddingBTransition.MakeIsVertInAcTabAndGetAcSize(endoFunctor, out isVertInAcTab);               
                if (IsAcSizeInRange(size, AcSize))
                {
                    for(int i = 0; i < size; i++)
                    {
                        unaryAutomata.TransitionFunctionsA[i] = (byte)endoFunctor[i];
                    }
                    AddingBTransition addingBTransition = new AddingBTransition(unaryAutomata, isVertInAcTab);
                    foreach (CoreDefinitions.IOptionalAutomaton acAutomata in addingBTransition.GenerateAc())
                    {
                        yield return acAutomata;
                    }
                }
            }
        }

        private static bool IsAcSizeInRange(int size, int AcSize)
        {
            return AcSize < size && 2 * AcSize >= size;
        }
    }
}
