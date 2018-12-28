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
    public static class ProgressIO
    {
        private const string savingAddress = "export.xml";
        public async static Task ExportStateAsync(UnaryAutomataDB database)
        {
            var exported = database.Export();
            var xsSubmit = new XmlSerializer(exported.GetType());
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, exported);
                    xml = sww.ToString();
                }
            }
            await File.WriteAllTextAsync(savingAddress, xml);
        }

        public static bool EnsureImportFileExistence() => File.Exists(savingAddress);

        public async static Task ImportStateAsync(UnaryAutomataDB database)
        {
            var xsSubmit = new XmlSerializer(typeof(List<FinishedStatistics>));
            var xml = await File.ReadAllTextAsync(savingAddress);
            
            using (var sr = new StreamReader(savingAddress))
            {
                database.ImportShallow((List<FinishedStatistics>)xsSubmit.Deserialize(sr));
            }
        }
    }
}
