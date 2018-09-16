using System;
using BinaryAutomataChecking;

namespace BinaryAutomataCheckingSomeTesting
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Hello World!");

            IUnaryAutomata unaryAutomata = new BinaryAutomata(new int[] { 1, 2, 2 });

            IBinaryAcAutomata binaryAcAutomata = (IBinaryAcAutomata)unaryAutomata;

            binaryAcAutomata.TransitionFunctionsB[2] = Byte.MaxValue;

            foreach(var automata in BinaryAutomataIterator.GetAllWithLongSynchronizedWord(4, 6))
            {
                AutomataPrinter.PrintAB(automata);
            }

            //foreach (var automata in UnaryGenetator.Generate(3))
            //{
            //    AutomataPrinter.PrintA(automata);
            //}

            //Console.WriteLine();
            //Console.WriteLine("-------------------------------------");
            //Console.WriteLine();

            //IUnaryAutomata unaryAutomata = new BinaryAutomata(new int[] { 2, 0, 0 });

            //foreach (var automata in unaryAutomata.MakeAcAutomatas())
            //{
            //    AutomataPrinter.PrintAB(automata);
            //}

            //Console.WriteLine();
            //Console.WriteLine("-------------------------------------");
            //Console.WriteLine();

            //IBinaryAcAutomata AcAutomata = new BinaryAutomata(new int[] { 2, 0, 0 });
            //AcAutomata.TransitionFunctionsB[0] = 2;
            //AcAutomata.TransitionFunctionsB[1] = Byte.MaxValue;
            //AcAutomata.TransitionFunctionsB[2] = 0;

            //foreach (var automata in AcAutomata.MakeFullAutomatas())
            //{
            //    AutomataPrinter.PrintAB(automata);
            //}

            

            Console.WriteLine();
        }
    }
}
