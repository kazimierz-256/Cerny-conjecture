﻿using System;
using System.Collections.Generic;

namespace CommunicationContracts
{
    public class ClientServerRequestForMoreAutomata
    {
        public SolvedAutomatonMessage solution;
        public int nextQuantity;
        public int suggestedMinimumBound;
    }

    public class SolvedAutomatonMessage
    {
        public int unaryIndex;
        public byte[] unaryArray;
        public List<ushort> solvedSyncLength;
        public List<byte[]> solvedB;
        public DateTime startTime;
        public DateTime finishTime;
    }
}
