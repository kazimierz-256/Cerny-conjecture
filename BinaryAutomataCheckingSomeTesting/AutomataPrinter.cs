using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryAutomataCheckingSomeTesting
{
    static class AutomataPrinter
    {
        public static void PrintA(CoreDefinitions.IOptionalAutomaton automata)
        {
            PrintAPart(automata.TransitionFunctionsA);
        }

        public static void PrintAB(CoreDefinitions.IOptionalAutomaton automata)
        {
            PrintAPart(automata.TransitionFunctionsA);
            PrintBPart(automata.TransitionFunctionsB);
        }

        public static void PrintABSolved(CoreDefinitions.ISolvedOptionalAutomaton automata)
        {
            Console.WriteLine($"Dlugosc slowa synchronizującego: {automata.SynchronizingWordLength}");
            Console.WriteLine("Automat:");
            PrintAPart(automata.TransitionFunctionsA);
            PrintBPart(automata.TransitionFunctionsB);
            Console.WriteLine();
        }

        private static void PrintAPart(byte[] transitionFunctionsA)
        {
            for (int i = 0; i < transitionFunctionsA.Length; i++)
            {
                Console.Write(i.ToString() + " ");
            }
            Console.WriteLine("a:");
            for (byte i = 0; i < transitionFunctionsA.Length; i++)
            {
                if (transitionFunctionsA[i] != Byte.MaxValue)
                    Console.WriteLine(agetArrow(i, transitionFunctionsA[i]));
            }
        }

        private static void PrintBPart(byte[] transitionFunctionsB)
        {
            Console.WriteLine("b:");
            for (byte i = 0; i < transitionFunctionsB.Length; i++)
            {
                if (transitionFunctionsB[i] != Byte.MaxValue)
                    Console.WriteLine(agetArrow(i, transitionFunctionsB[i]));
            }
            Console.WriteLine();
        }

        private static String agetArrow(byte from, byte to)
        {
            if (from == to)
                return getSequence(0, from, " ") + "O";
            if (to > from)
                return getSequence(0, from, " ") + getSequence(from, to, "-") + ">";
            else
                return getSequence(0, to, " ") + "<" + getSequence(to, from, "-");
        }

        private static String getSequence(byte from, byte to, String of)
        {
            String s = "";
            for (byte i = from; i < to; i++)
            {
                if (i / 10 == 0)
                    s += of + of;
                else
                    s += of + of + of;
            }
            return s;
        }
    }
}
