using System;
using BinaryAutomataChecking;

namespace BinaryAutomataCheckingSomeTesting
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Hello World!");

            //IUnaryAutomata unaryAutomata = new BinaryAutomata(new int[] { 1, 2, 2 });

            //IBinaryAcAutomata binaryAcAutomata = (IBinaryAcAutomata)unaryAutomata;

            //binaryAcAutomata.TransitionFunctionsB[2] = Byte.MaxValue;

            //foreach(var automata in BinaryAutomataIterator.GetAllWithLongSynchronizedWord(16, 6))
            //{
            //    AutomataPrinter.PrintABSolved(automata);
            //}
            for (int i = 3; i < 14; i++)
            {
                DateTime dateTimeStart = DateTime.Now;
                Console.WriteLine($"Start time: {dateTimeStart}");
                ulong count = 0;
                foreach (var automata in BinaryAutomataIterator.GetAllFullAutomatasToCheck(i))
                {
                    count++;
                    if (count % 100000000 == 0)
                    {
                        Console.WriteLine($"i = {i}, Count = {count}");
                        if (count % 1000000000 == 0)
                        {
                            Console.WriteLine($"Time {DateTime.Now}");
                            AutomataPrinter.PrintAB(automata);
                        }
                    }
                }
                DateTime dateTimeEnd = DateTime.Now;
                Console.WriteLine($"For size {i} there is {count} fullAutomata to check.");
                Console.WriteLine($"Time to generate them is {dateTimeEnd - dateTimeStart}");
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
