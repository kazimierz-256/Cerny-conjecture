using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinaryAutomataChecking
{
    static public class BinaryAutomataIterator
    {
        public static IEnumerable<IBinaryAutomata> GetAllWithLongSynchronizedWord(int minimalLenght, int size)
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
                from automata in solutionMapperReusable.SelectAsSolved(GetAllAcAutomatasToCheck(size))
                where automata.SynchronizingWordLength != null
                where automata.SynchronizingWordLength > minimalLenght
                select (BinaryAutomata)automata;
        }

        public static IEnumerable<IBinaryAutomata> GetAllFullAutomatasToCheck(int size)
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
            IEnumerable<IBinaryAcAutomata> AcAutomatas = 
                from automata in solutionMapperReusable.SelectAsSolved(GetAllAcAutomatasToCheck(size))
                where automata.SynchronizingWordLength==null || 2 * automata.SynchronizingWordLength + 1 > maxLenght
                select (BinaryAutomata)automata;

            foreach (var AcAutomat in AcAutomatas)
            {
                foreach(var fullAutomat in AcAutomat.MakeFullAutomatas())
                {
                    yield return fullAutomat;
                }
            }
        }

        public static IEnumerable<IBinaryAcAutomata> GetAllAcAutomatasToCheck (int size)
        {
            foreach (IUnaryAutomata unaryAutomata in UnaryGenetator.Generate(size))
            {
                foreach (IBinaryAcAutomata acAutomata in unaryAutomata.MakeAcAutomatas())
                {
                    if (IsAcSizeInRange(size, acAutomata.AcSize))
                        yield return acAutomata;             
                }
            }
        }

        private static bool IsAcSizeInRange(int size, byte AcSize)
        {
            return AcSize < size && 2 * AcSize >= size;
        }
    }
}
