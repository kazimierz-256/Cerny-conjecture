﻿using AutomataIterator;
using AutomataIteratorTests;
using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AutomataIteratorExperimentalBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var factories = new Func<ISolutionMapperReusable>[]
            {
                () =>  new PowerAutomatonReusableSolutionMapperMaximum12(),
                () =>  new PowerAutomatonReusableSolutionMapperFastMaximum12(),
                () =>  new PowerAutomatonReusableSolutionMapperUltraMaximum12_4bits(),
                () =>  new PowerAutomatonReusableSolutionMapperUltraMaximum12_3bits()
            };

            #region Serial Benchmark
            Console.WriteLine("Single thread benchmarks:");
            const int problems = 500_000;
            for (int exercise = 0; exercise < 10; exercise++)
            {
                Console.WriteLine();

                foreach (var solver in factories.Select(generator => generator()))
                {
                    var problemGenerator = RandomProblemGenerator.Generate(12, 12345).Take(problems);//.Select(problem => problem.DeepClone()).ToArray();

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    int problemCount = 0;
                    foreach (var result in solver.SelectAsSolved(problemGenerator))
                    {
                        problemCount += 1;
                    }
                    stopWatch.Stop();
                    Console.WriteLine($"solver {solver.GetType().Name} has speed {problems / stopWatch.Elapsed.TotalSeconds}");
                }
            }
            #endregion

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            #region Parallel Benchmark
            Console.WriteLine("Multiple thread benchmarks:");
            var threadCount = Environment.ProcessorCount;
            const int problemsPerThread = 1_000_000;

            for (int exercise = 0; exercise < 20; exercise++)
            {
                Console.WriteLine();

                foreach (var factory in factories)
                {
                    var threadSolvers = new ISolutionMapperReusable[threadCount];
                    var threadProducers = new IEnumerable<IOptionalAutomaton>[threadCount];
                    for (int i = 0; i < threadCount; i++)
                    {
                        threadSolvers[i] = factory();
                        threadProducers[i] = RandomProblemGenerator.Generate(12, i).Take(problemsPerThread);
                    }

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var total = 0;
                    Parallel.For(0, threadCount, threadIndex =>
                    {
                        int totalPerThread = 0;
                        foreach (var solution in threadSolvers[threadIndex].SelectAsSolved(threadProducers[threadIndex]))
                        {
                            totalPerThread += 1;
                        }
                        Interlocked.Add(ref total, totalPerThread);
                    });
                    stopWatch.Stop();
                    if (total != problemsPerThread * threadSolvers.Length)
                    {
                        throw new Exception("Computed different number of automatons!");
                    }
                    Console.WriteLine($"solver {threadSolvers.First().GetType().Name} has speed {total / stopWatch.Elapsed.TotalSeconds}");
                }
            }
            #endregion
        }
    }
}
