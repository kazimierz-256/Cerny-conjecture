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
        public List<double> speedStatistics;
    }

    public class FinishedStatistics
    {
        public SolvedAutomatonMessage solution;
        public string clientID;
    }
}
