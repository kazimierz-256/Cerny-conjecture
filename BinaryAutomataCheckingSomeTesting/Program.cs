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


            //for (int i = 3; i < 14; i++)
            int i = 11;
            {
                Console.WriteLine($"For size {i} there is {BinaryAutomataIterator.UnaryCount(i, 0, 10)} unaryAutomata to check.");
                Console.WriteLine($"For size {i} there is {BinaryAutomataIterator.UnaryCount(i, 0)} unaryAutomata to check.");
                DateTime dateTimeStart = DateTime.Now;
                Console.WriteLine($"Start time: {dateTimeStart}");
                ulong count = 0;


                foreach (var automata in BinaryAutomataIterator.GetAllWithLongSynchronizedWord(20, i, 4550, 4))
                {
                    // todo: gromadzimy statystyki o tym co sie poszczesci tzn. dlugosc slowa synchr + ile takich automatow
                    count++;
                    if (count % 1000000 == 0)
                    {
                        Console.WriteLine($"i = {i}, Count = {count}");
                        if (count % 20000000 == 0)
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
