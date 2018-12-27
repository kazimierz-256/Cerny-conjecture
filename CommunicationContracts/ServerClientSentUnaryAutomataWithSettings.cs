using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationContracts
{
    public class ServerClientSentUnaryAutomataWithSettings
    {
        public int targetCollectionSize;
        public int automatonSize;
        public int serverMinimalLength;
        public List<int> unaryAutomataIndices;
        public int targetTimeoutSeconds;
    }
}
