using AutomataIterator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryAutomataChecking
{
    public class BinaryAutomataIterator
    {
        private readonly ISolutionMapperReusable solutionMapper1;
        private readonly ISolutionMapperReusable solutionMapper2;
        public BinaryAutomataIterator()
        {
            solutionMapper1 = ExtensionMapProblemsToSolutions.GetNewMapper();
            solutionMapper2 = ExtensionMapProblemsToSolutions.GetNewMapper();
        }

        public IEnumerable<CoreDefinitions.ISolvedOptionalAutomaton> GetAllSolved(int size, int index)
            => GetAllSolvedRecursively(size, index);

        public IEnumerable<CoreDefinitions.ISolvedOptionalAutomaton> GetAllSolvedRecursively(int size, int index)
            => solutionMapper1.SelectAsSolved(GetAllFullAutomataToCheckRecursively(size, index));

        private IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllFullAutomataToCheckRecursively(int size, int index)
        {
            byte[] MemoryB = new byte[size], memoryHelp ;
            int maxLength = (size - 1) * (size - 1);
            List<byte>[] TransitionsFromA = new List<byte>[size];
            List<byte>[] helpList = new List<byte>[size];
            for (byte i = 0; i < size; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }

            foreach (var AcAutomaton in solutionMapper2.SelectAsSolved(GetAllAcAutomataToCheckWithMemory(size, index)))
            {
                if (AcAutomaton.SynchronizingWordLength == null || (AcAutomaton.SynchronizingWordLength * 2) + 1 > maxLength)
                {
                    Array.Copy(AcAutomaton.TransitionFunctionsB, MemoryB, AcAutomaton.TransitionFunctionsB.Length);
                    MakingFullAutomata makingFullAutomata = new MakingFullAutomata(AcAutomaton, TransitionsFromA, helpList);
                    foreach (var fullAutomaton in makingFullAutomata.Generate())
                    {
                        yield return fullAutomaton;
                    }
                    
                    memoryHelp = AcAutomaton.TransitionFunctionsB;
                    AcAutomaton.TransitionFunctionsB = MemoryB;
                    MemoryB = memoryHelp;
                }
            }
        }

        public IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllAcAutomataToCheckWithMemory(int size, int index)
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

        //nadpisywanie tablicy której jeszcze potrzeba
        public IEnumerable<CoreDefinitions.IOptionalAutomaton> GetAllAcAutomataToCheck(int size, int index)
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

        private static bool IsAcSizeInRange(int size, int AcSize)
        {
            return AcSize < size && 2 * AcSize >= size;
        }

        public static int UnaryCount(int size, int startIndex, int count = 1)
        {
            int unaryCount = 0;
            var endofunctors = Enumerable
                .Range(startIndex, count)
                .Select(index => UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(size, index))
                .Take(count);
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

        private void DeepCopyArray(byte[] tabFrom, byte[] tabTo)
        {
            for (int i = 0; i < tabFrom.Length; i++)
            {
                tabTo[i] = tabFrom[i];
            }
        }
    }
}
