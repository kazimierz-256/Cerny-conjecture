using System;
using System.Linq;
using Xunit;

namespace NonisomorphicEndofunctorsTests
{
    public class Isomorphism
    {
        [Theory]
        [InlineData(8, 100, 100, 29)]
        public void IsomorphicUnaryAutomata(int size, int samples, int permutingSamples, int seed)
        {
            var random = new Random(seed);
            for (int i = 0; i < samples; i += 1)
            {
                var automaton = GenerateRandomAutomaton(size, random);
                for (int j = 0; j < permutingSamples; j += 1)
                {
                    var permuted = GeneratePermutedAutomaton(automaton, random);
                    var isomorphic = UniqueUnaryAutomata.AutomatonIsomorphism.AreAutomataIsomorphic(automaton, permuted);
                    Assert.True(isomorphic);
                }
            }
        }

        [Theory]
        [InlineData(7)]
        public void UnaryNonisomorphicAutomataGenerator(int maxSize)
        {
            for (int i = 0; i < UniqueUnaryAutomata.Generator.theory[maxSize - 1]; i++)
            {
                for (int j = 0; j < UniqueUnaryAutomata.Generator.theory[maxSize - 1]; j++)
                {
                    var automatonI = UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(maxSize, i);
                    var automatonJ = UniqueUnaryAutomata.Generator.GetUniqueAutomatonFromCached(maxSize, j);

                    var iIsomorphicToJ = UniqueUnaryAutomata.AutomatonIsomorphism.AreAutomataIsomorphic(automatonI, automatonJ);

                    if (i == j)
                        Assert.True(iIsomorphicToJ);
                    else
                        Assert.False(iIsomorphicToJ);
                }
            }
        }

        int[] GenerateRandomAutomaton(int size, Random random)
        {
            var automaton = new int[size];
            for (int i = 0; i < size; i += 1)
                automaton[i] = random.Next(size);
            return automaton;
        }

        int[] GeneratePermutedAutomaton(int[] automaton, Random random)
        {
            var newNames = new int[automaton.Length];
            var randoms = new int[automaton.Length];
            for (int i = 0; i < automaton.Length; i += 1)
            {
                newNames[i] = i;
                randoms[i] = random.Next();
            }
            Array.Sort(randoms, newNames);
            var permuted = new int[automaton.Length];
            for (int i = 0; i < automaton.Length; i += 1)
            {
                permuted[newNames[i]] = newNames[automaton[i]];
            }
            return permuted;
        }
    }
}
