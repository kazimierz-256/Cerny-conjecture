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
        [InlineData(new byte[] { 4, 5, 6, 0, 0, 7, 1, 2 }, new byte[] { 7, 2, 4, 255, 0, 1, 5, 6 })]
        [InlineData(new byte[] { 4, 5, 6, 0, 0, 7, 1, 2 }, new byte[] { 7, 0, 2, 255, 1, 5, 4, 6 })]
        public void CheckIfDifferentFullFromAcAutomataRecursively(byte[] a_tab, byte[] b_tab)
        {
            int size = a_tab.Length;
            List<byte>[] TransitionsFromA = new List<byte>[size];
            List<byte>[] helpList = new List<byte>[size];
            for (byte i = 0; i < size; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }
            IOptionalAutomaton Ac = new OptionalAutomaton(a_tab, b_tab);
            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(Ac, TransitionsFromA,helpList);

            IEnumerable<IOptionalAutomaton> FullAutomata = makingFullAutomata.Generate();
            IEnumerable<byte[]> fullAutomata2 =
                from a in FullAutomata
                select CopyArray(a.TransitionFunctionsB);
            byte[][] bTransitions = fullAutomata2.ToArray();
            for (int i = 0; i < bTransitions.Length - 1; i++)
            {
                for (int j = i + 1; j < bTransitions.Length; j++)
                {
                    Assert.NotEqual(bTransitions[i], bTransitions[j]);
                }
            }
        }
        
        [Theory]
        //[InlineData(8, 849)] //wykomentowane gdy¿ trwa bardzo d³ugo, ale przechodzi
        [InlineData(7, 263)]

        public void CheckIfDifferentAcAutomata(int size, int index)
        {
            IEnumerable<IOptionalAutomaton> optionalAutomatons = new BinaryAutomataIterator().GetAllAcAutomataToCheck(size, index);
            int maxLength = (size - 1) * (size - 1);
            AutomataIterator.ISolutionMapperReusable solutionMapper2 = AutomataIterator.ExtensionMapProblemsToSolutions.GetNewMapper();
            List<byte[]>[] ListAc = new List<byte[]>[size];
            for (int i = 0; i < size; i++)
            {
                ListAc[i] = new List<byte[]>();
            }
            foreach (var AcAutomaton in solutionMapper2.SelectAsSolved(optionalAutomatons))
            {
                if (AcAutomaton.SynchronizingWordLength == null || (AcAutomaton.SynchronizingWordLength * 2) + 1 > maxLength)
                {
                    ListAc[AcAutomaton.TransitionFunctionsB[0]].Add(CopyArray(AcAutomaton.TransitionFunctionsB));
                }
            }
            byte[][][] TabSolvedOptionalAutomatons = new byte[10][][];
            for (int i = 0; i < size; i++)
            {
                TabSolvedOptionalAutomatons[i] = ListAc[i].ToArray();
            }
            for (int k = 0; k < size; k++)
            {
                for (int i = 0; i < TabSolvedOptionalAutomatons[k].Length - 1; i++)
                {
                    for (int j = i + 1; j < TabSolvedOptionalAutomatons[k].Length; j++)
                    {
                        Assert.NotEqual(TabSolvedOptionalAutomatons[k][i], TabSolvedOptionalAutomatons[k][j]);
                    }
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
            int size = tab_a.Length;
            List<byte>[] TransitionsFromA = new List<byte>[size];
            List<byte>[] helpList = new List<byte>[size];
            for (byte i = 0; i < size; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }
            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a,TransitionsFromA,helpList);

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
        [InlineData(new byte[] { 1, 0 }, new byte[] { 0, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { CoreDefinitions.OptionalAutomaton.MissingTransition, 1, 1 },
            new byte[] { 0, 0, 0 },
            new byte[] { 1, 0, 0 },
            new byte[] { 2, 0, 0 })]
        public void MakeFullAutomata(byte[] a_tab, byte[] b_tab, params byte[][] expectedTabs)
        {
            int size = a_tab.Length;
            List<byte>[] TransitionsFromA = new List<byte>[size];
            List<byte>[] helpList = new List<byte>[size];
            for (byte i = 0; i < size; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);

            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a,TransitionsFromA,helpList);

            IEnumerable<IOptionalAutomaton> FullAutomata = makingFullAutomata.Generate();
            IEnumerable<IOptionalAutomaton> FullAutomataIncremental = makingFullAutomata.Generate();

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
            int size = a_tab.Length;
            List<byte>[] TransitionsFromA = new List<byte>[size];
            List<byte>[] helpList = new List<byte>[size];
            for (byte i = 0; i < size; i++)
            {
                TransitionsFromA[i] = new List<byte>();
            }
            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a,TransitionsFromA,helpList);

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
