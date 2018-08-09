using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface IOptionalAutomaton : IOptionalAlgorithmDefinition
    {
        ISolvedOptionalAutomaton CreateSolvedObject(bool isSynchronizable, ushort synchronizingWordLength);
    }
}
