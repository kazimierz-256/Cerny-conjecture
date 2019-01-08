using CommunicationContracts;
using CoreServer.UnaryAutomataDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CoreServer.ProgressIO
{
    public class ProgressIO
    {
        public List<FinishedStatistics> finishedStatistics;
        public int Size;
        internal int MaximumLongestAutomataCount;
        internal int AllowedCount;
        private const string savingAddress = "export.xml";
        private static string GetAddress(UnaryAutomataDB database) => $"{database.Size}-{database.MaximumLongestAutomataCount}-{savingAddress}";
        public async static Task ExportStateAsync(UnaryAutomataDB database)
        {
            var exported = database.Export();
            var serializer = new XmlSerializer(exported.GetType());

            using (var sw = new StringWriter())
            {
                using (var w = XmlWriter.Create(sw))
                {
                    serializer.Serialize(w, exported);
                    await File.WriteAllTextAsync(GetAddress(database), sw.ToString());
                }
            }
        }

        public static void ImportStateIfPossible(UnaryAutomataDB database)
        {
            var address = GetAddress(database);
            if (File.Exists(address))
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
            else
            {
                Console.WriteLine("no computation history or corrupt");
            }
        }
    }
}
