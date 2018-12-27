using System;
using System.Collections.Generic;
using System.Diagnostics;
using BinaryAutomataChecking;

namespace BinaryAutomataCheckingSomeTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello User!");


            ShowCalculation(10, 100);


            Console.WriteLine();
        }

        private static void ShowCalculation(int size, int index)
        {
            var displayInterval = TimeSpan.FromSeconds(2);
            Console.WriteLine($"For size {size} there is {BinaryAutomataIterator.UnaryCount(size, index)} unaryAutomata to check.");
            DateTime dateTimeStart = DateTime.Now;
            ulong count = 0;
            ulong totalCount = 0;
            int wordLength = (size - 1) * (size - 1) / 7;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var previouslyDisplayed = DateTime.MinValue;
            foreach (var automaton in new BinaryAutomataIterator().GetAllSolved(size, index))
            {
                totalCount += 1;

                if (automaton.SynchronizingWordLength.HasValue && automaton.SynchronizingWordLength.Value >= wordLength)
                {
                    // gromadzimy statystyki o tym co sie poszczesci tzn. dlugosc slowa synchr + ile takich automatow
                    count += 1;
                }

                if (totalCount % 1000000 == 0)
                {
                    previouslyDisplayed = DateTime.Now;
                    Console.WriteLine($"size = {size}, Count = {count}");
                    Console.WriteLine($"Time {DateTime.Now}");
                    Console.WriteLine($"Total per second {totalCount / stopwatch.Elapsed.TotalSeconds:F1}");
                    //AutomataPrinter.PrintAB(automaton);
                    Console.WriteLine($"Synchronizing word length: {automaton.SynchronizingWordLength}");
                    Console.WriteLine();
                }
            }
            DateTime dateTimeEnd = DateTime.Now;
            Console.WriteLine($"For size {size}, start index {index} and count {1}\nthere is {count} fullAutomata with word length at least {wordLength}.");
            Console.WriteLine($"Time to generate them is {dateTimeEnd - dateTimeStart}");
        }


    }
}
