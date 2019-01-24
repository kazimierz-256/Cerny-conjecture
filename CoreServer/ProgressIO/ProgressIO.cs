using CommunicationContracts;
using CoreServer.UnaryAutomataDatabase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CoreServer.ProgressIO
{
    public static class ProgressIO
    {
        private static string GetDetailedAddress(int size, int count, int found) => $"computation-summary-{size}-{count}-{found}.xml";
        private static object syncObject = new object();
        private static HashSet<string> stillAvailable = new HashSet<string>();
        public async static Task ExportStateAsync(UnaryAutomataDB database)
        {
            var exported = database.ExportAsXMLString();
            var serializer = new XmlSerializer(exported.GetType());

            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw))
                {
                    serializer.Serialize(w, exported);
                    var found = exported.finishedStatistics.Count(finished => finished.solved);
                    var detailedAddress = GetDetailedAddress(exported.Size, exported.MaximumLongestAutomataCount, found);
                    var swString = sw.ToString();
                    var toDelete = new List<int>();
                    try
                    {
                        lock (syncObject)
                        {
                            File.WriteAllTextAsync(detailedAddress, swString).Wait();
                            foreach (var address in stillAvailable)
                            {
                                if (File.Exists(address))
                                    File.Delete(address);
                            }
                            stillAvailable.Clear();
                            if (found < database.Total)
                                stillAvailable.Add(detailedAddress);
                        }
                        Console.WriteLine($"Successfully exported database having computed {database.Found} unary automata at {DateTime.Now} to a file {detailedAddress}. Deleted the rest");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public static void ImportStateIfPossible(UnaryAutomataDB database, string address)
        {
            if (address != null && address != string.Empty && File.Exists(address))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(DBSerialized));

                    using (var sr = new StreamReader(address))
                    {
                        var obj = serializer.Deserialize(sr);
                        var data = (DBSerialized)obj;

                        if (database.Size == data.Size)
                            database.ImportShallow(data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("error importing database!");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("no computation history or corrupt");
            }
        }
    }
}
