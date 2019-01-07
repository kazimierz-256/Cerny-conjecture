using System;
using System.Collections.Generic;
using System.Text;
using CoreDefinitions;

namespace CommunicationContracts
{
    public class ServerPresentationComputationSummary
    {
        public string description;
        public List<FinishedStatistics> finishedAutomata;
        public int total;
    }

    public class FinishedStatistics
    {
        public SolvedAutomatonMessage solution;
        public DateTime finishTime;
        public DateTime issueTime;
        public string clientID;
    }
}
