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
    public static class ProgressIO
    {
        private static string GetDetailedAddress(UnaryAutomataDB database) => $"all-history-{database.Size}-{database.MaximumLongestAutomataCount}-{database.Found}.xml";
        private static string GetDetailedPreviousAddress(UnaryAutomataDB database) => $"all-history-{database.Size}-{database.MaximumLongestAutomataCount}-{database.Found - 1}.xml";
        private static Semaphore syncObject = new Semaphore(1, 1);
        public async static Task ExportStateAsync(UnaryAutomataDB database)
        {
            var exported = database.ExportAsXMLString();
            var serializer = new XmlSerializer(exported.GetType());

            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw))
                {
                    serializer.Serialize(w, exported);
                    var detailedAddress = GetDetailedAddress(database);
                    var previousAddress = GetDetailedPreviousAddress(database);
                    var swString = sw.ToString();
                    try
                    {
                        syncObject.WaitOne();
                        await File.WriteAllTextAsync(detailedAddress, swString);
                        if (File.Exists(previousAddress))
                            File.Delete(previousAddress);
                        syncObject.Release();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    Console.WriteLine($"Successfully exported database having computed {database.Found} unary automata at {DateTime.Now} to a file {detailedAddress}.");
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
