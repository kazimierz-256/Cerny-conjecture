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
        };

        [Fact]
        public void CernyAutomataTestSmall()
        {
            const int nMax = 12;
            // if internal const in solver is 13 then cerny problem generator is allowed to take only 11 first automata
            var cernyProblems = CernyProblemGenerator.Generate().Take(nMax - 2);
            foreach (var solver in solvers)
            {
                foreach (var automaton in solver.SelectAsSolved(cernyProblems))
                {
                    var n = automaton.TransitionFunctionsB.Length;
                    Assert.True(automaton.IsSynchronizable);
                    Assert.Equal((n - 1) * (n - 1), automaton.SynchronizingWordLength);
                }
            }
        }

        private void VerifyIntegrity(IEnumerable<IOptionalAutomaton> problems)
        {
            var iterators = solvers.Select(solver => solver.SelectAsSolved(problems).GetEnumerator()).ToArray();
            while (iterators[0].MoveNext())
            {
                var referenceResult = iterators[0].Current;
                for (int iteratorIndex = 1; iteratorIndex < iterators.Length; iteratorIndex++)
                {
                    iterators[iteratorIndex].MoveNext();
                    Assert.Equal(referenceResult.IsSynchronizable, iterators[iteratorIndex].Current.IsSynchronizable);
                    if (referenceResult.IsSynchronizable)
                    {
                        Assert.Equal(referenceResult.SynchronizingWordLength, iterators[iteratorIndex].Current.SynchronizingWordLength);
                    }
                }
            }
        }

        [Fact]
        public void RandomAutomataIntegrityTest()
        {
            const int problemCountPerN = 100_000;
            const int seed = 1234567;
            for (int n = 12; n >= 1; n -= 1)
            {
                VerifyIntegrity(RandomProblemGenerator.Generate(n, seed).Take(problemCountPerN));
            }
        }

        [Fact]
        public void BasicCountMatch()
        {
            const int problemCountPerN = 10_000;
            const int seed = 87654321;
            for (int n = 12; n >= 1; n -= 1)
            {
                var solutions = RandomProblemGenerator.Generate(n, seed).Take(problemCountPerN).SelectAsSolved();
                Assert.Equal(problemCountPerN, solutions.Count());
            }
        }
    }
}
