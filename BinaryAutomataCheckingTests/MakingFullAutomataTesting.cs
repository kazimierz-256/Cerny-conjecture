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

        //[Theory]
        //[InlineData(2,
        //    new int[] { 0, 0 },
        //    new int[] { 1, 0 },
        //    new int[] { 0, 1 })]
        //public void MakeUnaryAutomata(int n, params int[][] tabs)
        //{
        //    List<int[]> expectedList = new List<int[]>(tabs);

        //    IEnumerable<int[]> UnaryAutomatas = UnaryGenetator.Generate(n);

        //    IEnumerable<int[]> AutomatasATransform =
        //        from automata in UnaryAutomatas
        //        select automata;

        //    Assert.Equal(expectedList, AutomatasATransform);
        //}

        [Theory]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { Byte.MaxValue, 1, 1 },
            new byte[] { },
            new byte[] { 0 },
            new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2, 1, 3 }, new byte[] { Byte.MaxValue, 1, 1, 3 },
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
            new bool[] { true,true}, 2)]
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
            new byte[] { Byte.MaxValue, 1, 1 },
            new byte[] { Byte.MaxValue, 1, 2 },
            new byte[] { Byte.MaxValue, 2, 1 },
            new byte[] { Byte.MaxValue, 2, 2 })]
        public void MakeAcAutomata(byte[] a_tab, byte[] b_tab, params byte[][] tabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);
            List<byte[]> expectedList = new List<byte[]>(tabs);

            bool[] isVertTab ;
            AddingBTransition.MakeIsVertInAcTabAndGetAcSize(a_tab, out isVertTab);

            AddingBTransition addingBTransition = new AddingBTransition(a, isVertTab);

            IEnumerable<IOptionalAutomaton> AcAutomatas = addingBTransition.GenerateAc();

            IEnumerable<byte[]> AcAutomatasBTransform =
                from automata in AcAutomatas
                select CopyArray(automata.TransitionFunctionsB);

            Assert.Equal(expectedList, AcAutomatasBTransform);
        }


        [Theory]
        [InlineData(new byte[] { 1, 0 }, new byte[] { 0, 0 },
            new byte[] { 1, 1 })]
        [InlineData(new byte[] { 1, 2, 2 }, new byte[] { Byte.MaxValue, 1, 1 },
            new byte[] { 0, 0, 0 },
            new byte[] { 1, 0, 0 },
            new byte[] { 2, 0, 0 })]
        public void MakeFullAutomata(byte[] a_tab, byte[] b_tab, params byte[][] tabs)
        {
            IOptionalAutomaton a = new OptionalAutomaton(a_tab, b_tab);
            List<byte[]> expectedList = new List<byte[]>(tabs);

            MakingFullAutomata makingFullAutomata = new MakingFullAutomata(a);

            IEnumerable<IOptionalAutomaton> FullAutomatas = makingFullAutomata.Generate();

            IEnumerable<byte[]> AcAutomatasBTransform =
                from automata in FullAutomatas
                select CopyArray(automata.TransitionFunctionsB);

            Assert.Equal(expectedList, AcAutomatasBTransform);
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
    }
}