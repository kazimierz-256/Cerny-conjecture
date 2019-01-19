using CommunicationContracts;
using CoreServer.UnaryAutomataDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CoreServer.ProgressIO
{
    public class ProgressIO
    {
        public FinishedStatistics[] finishedStatistics;
        public int Size;
        public int MaximumLongestAutomataCount;
        public int AllowedCount;
        private static string GetAddress(UnaryAutomataDB database) => $"all-history-{database.Size}-{database.MaximumLongestAutomataCount}.xml";
        private static string GetDetailedAddress(UnaryAutomataDB database) => $"all-history-{database.Size}-{database.MaximumLongestAutomataCount}-{database.Found}.xml";
        private static Semaphore syncObject = new Semaphore(1, 1);
        public async static Task ExportStateAsync(UnaryAutomataDB database)
        {
            var exported = database.Export();
            var serializer = new XmlSerializer(exported.GetType());

            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw))
                {
                    serializer.Serialize(w, exported);
                    var address = GetAddress(database);
                    var detailedAddress = GetDetailedAddress(database);
                    var swString = sw.ToString();
                    syncObject.WaitOne();
                    await File.WriteAllTextAsync(address, swString);
                    await File.WriteAllTextAsync(detailedAddress, swString);
                    syncObject.Release();
                    Console.WriteLine($"Successfully exported database having computed {database.Found} unary automata at {DateTime.Now} to a file {address}.");
                }
            }
        }

        public static void ImportStateIfPossible(UnaryAutomataDB database)
        {
            var address = GetAddress(database);
            if (File.Exists(address))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(ProgressIO));

                    using (var sr = new StreamReader(address))
                    {
                        var obj = serializer.Deserialize(sr);
                        var data = (ProgressIO)obj;

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
