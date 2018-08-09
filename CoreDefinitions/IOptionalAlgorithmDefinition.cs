using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface IOptionalAlgorithmDefinition
    {
        byte[] TransitionFunctionsB { get; }
        byte[] TransitionFunctionsA { get; }
    }
}
