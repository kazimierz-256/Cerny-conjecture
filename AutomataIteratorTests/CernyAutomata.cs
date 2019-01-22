using AutomataIterator;
using CoreDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutomataIteratorTests
{
    public class CernyAutomata
    {
        private readonly ISolutionMapperReusable[] solvers = new ISolutionMapperReusable[]
        {
            new PowerAutomatonReusableSolutionMapperMaximum12(),
            new PowerAutomatonReusableSolutionMapperFastMaximum12()
            // additionally the brute force power automaton
        };

        [Theory]
        [InlineData(12)]
        public void CernyAutomataTestSmall(int nMax)
        {
            // if internal const in solver is 13 then cerny problem generator is allowed to take only 11 first automata
            var cernyProblems = CernyProblemGenerator.Generate().Take(nMax - 2);
            foreach (var solver in solvers)
            {
                foreach (var automaton in solver.SelectAsSolved(cernyProblems))
                {
                    var n = automaton.TransitionFunctionsB.Length;
                    Assert.True(automaton.SynchronizingWordLength.HasValue);
                    Assert.Equal((n - 1) * (n - 1), automaton.SynchronizingWordLength.Value);
                }
            }
        }

        private void VerifyIntegrity(IEnumerable<ISolutionMapperReusable> solvers, IEnumerable<IOptionalAutomaton> problems)
        {
            var iterators = solvers.Select(solver => solver.SelectAsSolved(problems).GetEnumerator()).ToArray();
            while (iterators[0].MoveNext())
            {
                var referenceResult = iterators[0].Current;
                for (int iteratorIndex = 1; iteratorIndex < iterators.Length; iteratorIndex++)
                {
                    iterators[iteratorIndex].MoveNext();
                    Assert.Equal(referenceResult.SynchronizingWordLength.HasValue, iterators[iteratorIndex].Current.SynchronizingWordLength.HasValue);
                    if (referenceResult.SynchronizingWordLength.HasValue)
                    {
                        Assert.Equal(referenceResult.SynchronizingWordLength, iterators[iteratorIndex].Current.SynchronizingWordLength);
                    }
                }
            }
        }

        [Theory]
        [InlineData(12, 20_000, 1234567)]
        public void RandomAutomataIntegrityTest(int n, int problemCountPerN, int seed)
        {
            for (; n >= 1; n -= 1)
                VerifyIntegrity(solvers, RandomProblemGenerator.Generate(n, seed).Take(problemCountPerN));
        }

        [Theory]
        [InlineData(5, 50, 1234567)]
        public void RandomExactAutomataIntegrityTest(int n, int problemCountPerN, int seed)
        {
            for (; n >= 1; n -= 1)
                VerifyIntegrity(solvers.Append(new PowerAutomatonReusableSolutionMapperSlowBruteForce()), RandomProblemGenerator.Generate(n, seed).Take(problemCountPerN));
        }


        [Fact]
        public void BasicCountMatch()
        {
            const int problemCountPerN = 10_000;
            const int seed = 87654321;
            for (int n = 12; n >= 1; n -= 1)
            {
                var solutions = RandomProblemGenerator.Generate(n, seed).SelectAsSolved().Take(problemCountPerN);
                Assert.Equal(problemCountPerN, solutions.Count());

                solutions = RandomProblemGenerator.Generate(n, seed + 1).Take(problemCountPerN).SelectAsSolved();
                Assert.Equal(problemCountPerN, solutions.Count());
            }

            var solutions2 = RandomProblemGenerator.Generate(16, seed + 2).SelectAsSolved<PowerAutomatonReusableSolutionMapperFastMaximum16>().Take(problemCountPerN);
            Assert.Equal(problemCountPerN, solutions2.Count());
        }
    }
}
