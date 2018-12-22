using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryAutomataChecking
{
    static public class BinaryAutomataIterator
    {
        public static IEnumerable<CoreDefinitions.ISolvedOptionalAutomaton> GetAllWithLongSynchronizedWord(Func<int> minimalLength, int size, int index)
        {
            foreach (var automaton in AutomataIterator.ExtensionMapProblemsToSolutions.SelectAsSolved(GetAllFullAutomatasToCheck(size, index)))
            {
                if (automaton.SynchronizingWordLength != null && automaton.SynchronizingWordLength > minimalLength())
                {
                    yield return automaton;
                }
            }
        }

        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllFullAutomatasToCheck(int size, int index)
        {
            int maxLength = (size - 1) * (size - 1);

            foreach (var AcAutomaton in AutomataIterator.ExtensionMapProblemsToSolutions.SelectAsSolved(GetAllAcAutomatasToCheck(size, index)))
            {
                if (AcAutomaton.SynchronizingWordLength == null || (AcAutomaton.SynchronizingWordLength << 1) + 1 > maxLength)
                {
                    MakingFullAutomata makingFullAutomata = new MakingFullAutomata(AcAutomaton);
                    foreach (var fullAutomaton in makingFullAutomata.Generate())
                    {
                        yield return fullAutomaton;
                    }
                }
            }
        }

        public static IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllAcAutomatasToCheck(int size, int index)
        {
            byte[] TranA = new byte[size], TranB = new byte[size];
            CoreDefinitions.IOptionalAutomaton unaryAutomata = new CoreDefinitions.OptionalAutomaton(TranA, TranB);

            var endoFunctor = UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(size, index);

            bool[] isVertInAcTab;
            int AcSize = AddingBTransition.MakeIsVertInAcTabAndGetAcSize(endoFunctor, out isVertInAcTab);
            if (IsAcSizeInRange(size, AcSize))
            {
                for (int i = 0; i < size; i++)
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

        public static int UnaryCount(int size, int startIndex, int count = 1)
        {
            int unaryCount = 0;
            var endofunctors = UniqueUnaryAutomata.Generator.GetAllUniqueAutomataOfSize(size).Skip(startIndex);
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
