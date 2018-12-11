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
            foreach (var automata in UniqueUnaryAutomata.Generator.EnumerateCollectionsOfNonisomorphicUnaryAutomata().Take(maxSize))
            {
                for (int i = 0; i < automata.Length - 1; i++)
                {
                    Assert.True(UniqueUnaryAutomata.AutomatonIsomorphism.AreAutomataIsomorphic(automata[i], automata[i]));
                    for (int j = i + 1; j < automata.Length; j += 1)
                    {
                        var iIsomorphicToJ = UniqueUnaryAutomata.AutomatonIsomorphism.AreAutomataIsomorphic(automata[i], automata[j]);
                        var jIsomorphicToI = UniqueUnaryAutomata.AutomatonIsomorphism.AreAutomataIsomorphic(automata[j], automata[i]);

                        Assert.False(iIsomorphicToJ);
                        Assert.False(jIsomorphicToI);
                    }
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
