using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryAutomataChecking
{
    static public class BinaryAutomataIterator
    {
        public static IEnumerable<CoreDefinitions.ISolvedOptionalAutomaton> GetAllWithLongSynchronizedWord(int minimalLenght, int size, int startIndex, int count)
        {
            return
                from automata in AutomataIterator.ExtensionMapProblemsToSolutions.SelectAsSolved(GetAllFullAutomatasToCheck(size, startIndex, count))
                where automata.SynchronizingWordLength != null
                where automata.SynchronizingWordLength > minimalLenght
                select automata;
        }

        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllFullAutomatasToCheck(int size, int startIndex, int count)
        {
            int maxLength = (size - 1) * (size - 1);
            IEnumerable<CoreDefinitions.IOptionalAutomaton> AcAutomatas = 
                from automata in AutomataIterator.ExtensionMapProblemsToSolutions.SelectAsSolved(GetAllAcAutomatasToCheck(size, startIndex, count))
                where automata.SynchronizingWordLength==null || 2 * automata.SynchronizingWordLength + 1 > maxLength
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

        // start inclusive
        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllAcAutomatasToCheck (int size, int startIndex, int count)
        {
            byte[] TranA = new byte[size], TranB = new byte[size];
            CoreDefinitions.IOptionalAutomaton unaryAutomata = new CoreDefinitions.OptionalAutomaton(TranA, TranB);

            UniqueUnaryAutomata.Generator endofunctorsGenerator = new UniqueUnaryAutomata.Generator();
            foreach (int[] endoFunctor in endofunctorsGenerator.GetAllUniqueAutomataOfSize(size).Skip(startIndex).Take(count))
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

        public static int UnaryCount(int size, int startIndex, int count = -1)
        {
            int unaryCount = 0;
            UniqueUnaryAutomata.Generator endofunctorsGenerator = new UniqueUnaryAutomata.Generator();
            var endofunctors = endofunctorsGenerator.GetAllUniqueAutomataOfSize(size).Skip(startIndex);
            if (count >= 0)
                endofunctors = endofunctors.Take(count);
            foreach (int[] endoFunctor in endofunctors)
            {
                bool[] isVertInAcTab;
                int AcSize = AddingBTransition.MakeIsVertInAcTabAndGetAcSize(endoFunctor, out isVertInAcTab);
                if (IsAcSizeInRange(size, AcSize))
                {
                    unaryCount++;
                }

            }
            return unaryCount;
        }

        private static bool IsAcSizeInRange(int size, int AcSize)
        {
            return AcSize < size && 2 * AcSize >= size;
        }
    }
}
