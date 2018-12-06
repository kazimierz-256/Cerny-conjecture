using System;
using BinaryAutomataChecking;

namespace BinaryAutomataCheckingSomeTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello User!");


            ShowCalculation(10, 51, 1);


            Console.WriteLine();
        }



        private static void ShowCalculation(int size, int startIndex, int countIndex=1)
        {

            Console.WriteLine($"For size {size} there is {BinaryAutomataIterator.UnaryCount(size, startIndex, countIndex)} unaryAutomata to check.");
            DateTime dateTimeStart = DateTime.Now;
            Console.WriteLine($"Start time: {dateTimeStart}");
            ulong count = 0;
            int wordLength  = (size - 1) * (size - 1) / 7;

            foreach (var automata in BinaryAutomataIterator.GetAllWithLongSynchronizedWord(wordLength, size, startIndex, countIndex))
            {
                // todo: gromadzimy statystyki o tym co sie poszczesci tzn. dlugosc slowa synchr + ile takich automatow
                count++;
                //if (count % 1000 == 0)
                {
                    Console.WriteLine($"size = {size}, Count = {count}");
                    //if (count % 200000 == 0)
                    {
                        Console.WriteLine($"Time {DateTime.Now}");
                        AutomataPrinter.PrintAB(automata);
                        Console.WriteLine($"Synchronizing word length: {automata.SynchronizingWordLength}");
                    }
                }
            }
            DateTime dateTimeEnd = DateTime.Now;
            Console.WriteLine($"For size {size}, start index {startIndex} and count {countIndex}\nthere is {count} fullAutomata with word length at least {wordLength}.");
            Console.WriteLine($"Time to generate them is {dateTimeEnd - dateTimeStart}");
        }


    }
}
