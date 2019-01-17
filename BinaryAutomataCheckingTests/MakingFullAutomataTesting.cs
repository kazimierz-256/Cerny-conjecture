using System;
using Xunit;
using BinaryAutomataChecking;
using CoreDefinitions;
using System.Collections.Generic;
using System.Linq;

namespace BinaryAutomataCheckingTests
{
    public class MakingFullAutomataTesting
    {
        [Theory]
        [InlineData(9,1549)]
        public void CheckIfDifferentFullAutomata(int size, int index)
        {
            IEnumerable<ISolvedOptionalAutomaton> solvedOptionalAutomatons = new BinaryAutomataIterator().GetAllSolved(size, index);
            IEnumerable<byte[]> solvedOptionalAutomatons2 =
                from a in solvedOptionalAutomatons
                where a.SynchronizingWordLength.HasValue && a.SynchronizingWordLength.Value > 23
                select CopyArray(a.TransitionFunctionsB);
            byte[][] TabSolvedOptionalAutomatons = solvedOptionalAutomatons2.ToArray();
            for(int i = 0; i < TabSolvedOptionalAutomatons.Length - 1; i++)
            {
                for (int j = i+1; j < TabSolvedOptionalAutomatons.Length; j++)
                {
                    Assert.NotEqual(TabSolvedOptionalAutomatons[i], TabSolvedOptionalAutomatons[j]);
                }
            }
        }

        [Theory]
        [InlineData(9, 1549)]
        public void CheckIfDifferentAcAutomata(int size, int index)
        {
            IEnumerable<IOptionalAutomaton> optionalAutomatons = new BinaryAutomataIterator().GetAllAcAutomataToCheck(size, index);
            IEnumerable<byte[]> solvedOptionalAutomatons2 =
                from a in optionalAutomatons
                select CopyArray(a.TransitionFunctionsB);
            byte[][] TabSolvedOptionalAutomatons = solvedOptionalAutomatons2.ToArray();
            for (int i = 0; i < TabSolvedOptionalAutomatons.Length - 1; i++)
            {
                for (int j = i + 1; j < TabSolvedOptionalAutomatons.Length; j++)
                {
                    Assert.NotEqual(TabSolvedOptionalAutomatons[i], TabSolvedOptionalAutomatons[j]);
                }
            }
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { },
            new byte[] { 0 },
            new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 1, 3 }, new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1, 3 },
            new byte[] { },
            new byte[] { 0, 2 },
            new byte[] { 1 },
            new byte[] { 3 })]
        public void MakeFromALists(byte[] tab_a, byte[] tab_b, params byte[][] lists)
        {
            IOptionalAutomaton a = new OptionalAutomaton(tab_a, tab_b);

            List<byte>[] expectedList = new List<byte>[lists.Length];
            for (int i = 0; i < expectedList.Length; i++)
            {
                expectedList[i] = new List<byte>(lists[i]);
            }

            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a);

            Assert.Equal(expectedList, makingFullAutomata.TransitionsFromA);
        }


        [Theory]
        [InlineData(new int[] { 0, 1 },
            new bool[] { true, true }, 2)]
        [InlineData(new int[] { 1, 2, 2 },
            new bool[] { false, true, true }, 2)]
        public void MakeIsVertInAcTab(int[] a_tab, bool[] isVertInAcExpected, int AcSizeExpec)
        {
            bool[] isVertInAc;
            int AcSize;

            AcSize = AddingBTransition.MakeIsVertInAcTabAndGetAcSize(a_tab, out isVertInAc);

            Assert.Equal(AcSizeExpec, AcSize);
            Assert.Equal(isVertInAcExpected, isVertInAc);
        }



        [Theory]
        [InlineData(new byte[] { 0, 1 }, new byte[] { 0, 0 },
            new byte[] { 0, 0 },
            new byte[] { 0, 1 },
            new byte[] { 1, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { 0, 0, 0 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 2 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 2, 1 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 2, 2 })]
        public void MakeAcAutomata(byte[] a_tab, byte[] b_tab, params byte[][] expectedTabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);

            bool[] isVertTab;
            AddingBTransition.MakeIsVertInAcTabAndGetAcSize(a_tab, out isVertTab);

            AddingBTransition addingBTransition = new AddingBTransition(a, isVertTab);

            IEnumerable<IOptionalAutomaton> AcAutomata = addingBTransition.GenerateAc();

            IEnumerable<byte[]> AcAutomataBTransform =
                from automata in AcAutomata
                select CopyArray(automata.TransitionFunctionsB);

            IsTheSame(expectedTabs, AcAutomataBTransform);
        }


        [Theory]
        [InlineData(new byte[] { 0, 1 }, new byte[] { 0, 0 },
            new byte[] { 0, 0 },
            new byte[] { 0, 1 },
            new byte[] { 1, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { 0, 0, 0 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 2 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 2, 1 },
            new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 2, 2 })]
        public void GenerateAcSameAsFast(byte[] a_tab, byte[] b_tab, params byte[][] expectedTabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);

            bool[] isVertTab;
            AddingBTransition.MakeIsVertInAcTabAndGetAcSize(a_tab, out isVertTab);

            AddingBTransition addingBTransition = new AddingBTransition(a, isVertTab);

            IEnumerable<IOptionalAutomaton> AcAutomata = addingBTransition.GenerateAc();
            IEnumerable<IOptionalAutomaton> AcAutomataIncremental = addingBTransition.GenerateAcIncrementally();

            var acIterator = AcAutomata.GetEnumerator();
            var acIncrementalIterator = AcAutomataIncremental.GetEnumerator();

            while (acIterator.MoveNext())
            {
                acIncrementalIterator.MoveNext();
                for (int i = 0; i < acIterator.Current.TransitionFunctionsB.Length; i++)
                {
                    Assert.Equal(acIterator.Current.TransitionFunctionsB[i], acIncrementalIterator.Current.TransitionFunctionsB[i]);
                }
            }
            Assert.False(acIncrementalIterator.MoveNext());
        }


        [Theory]
        [InlineData(new byte[] { 1, 0 }, new byte[] { 0, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { 0, 0, 0 },
            new byte[] { 1, 0, 0 },
            new byte[] { 2, 0, 0 })]
        public void MakeFullAutomata(byte[] a_tab, byte[] b_tab, params byte[][] expectedTabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);

            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a);

            IEnumerable<IOptionalAutomaton> FullAutomata = makingFullAutomata.Generate();
            IEnumerable<IOptionalAutomaton> FullAutomataIncremental = makingFullAutomata.GenerateIncrementally();

            var fullIterator = FullAutomata.GetEnumerator();
            var fullIncrementalIterator = FullAutomataIncremental.GetEnumerator();

            while (fullIterator.MoveNext())
            {
                fullIncrementalIterator.MoveNext();
                for (int i = 0; i < fullIterator.Current.TransitionFunctionsB.Length; i++)
                {
                    Assert.Equal(fullIterator.Current.TransitionFunctionsB[i], fullIncrementalIterator.Current.TransitionFunctionsB[i]);
                }
            }
            Assert.False(fullIncrementalIterator.MoveNext());
        }
        [Theory]
        [InlineData(new byte[] { 1, 0 }, new byte[] { 0, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { 0, 0, 0 },
            new byte[] { 1, 0, 0 },
            new byte[] { 2, 0, 0 })]
        public void VerifyMatchingIterativeGeneration(byte[] a_tab, byte[] b_tab, params byte[][] expectedTabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);

            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a);

            IEnumerable<IOptionalAutomaton> FullAutomata = makingFullAutomata.Generate();

            IEnumerable<byte[]> AcAutomataBTransform =
                from automata in FullAutomata
                select CopyArray(automata.TransitionFunctionsB);

            IsTheSame(expectedTabs, AcAutomataBTransform);
        }

        byte[] CopyArray(byte[] tab)
        {
            byte[] newArray = new byte[tab.Length];
            for (int i = 0; i < tab.Length; i++)
            {
                newArray[i] = tab[i];
            }
            return newArray;
        }

        private bool IsTheSame(IEnumerable<byte[]> expected, IEnumerable<byte[]> actual)
        {
            Assert.Equal(expected.Count(), actual.Count());
            foreach (var tab in expected)
                Assert.Contains(tab, actual);
            return true;
        }
    }
}
