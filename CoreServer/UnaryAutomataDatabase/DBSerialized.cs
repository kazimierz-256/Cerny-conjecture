using CommunicationContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreServer.UnaryAutomataDatabase
{
    public class DBSerialized
    {
        public FinishedStatistics[] finishedStatistics;
        public int Size;
        public int MaximumLongestAutomataCount;
        public int AllowedCount;
    }
}
