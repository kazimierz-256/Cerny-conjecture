using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface IOptionalAutomaton
    {
        byte[] TransitionFunctionsB { get; set; }
        byte[] TransitionFunctionsA { get; }

        IOptionalAutomaton DeepClone();
    }
}
