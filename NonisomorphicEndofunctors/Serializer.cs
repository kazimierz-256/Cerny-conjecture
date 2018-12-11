using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniqueUnaryAutomata
{
    public static class Serializer
    {
        public static void SerializeAutomataOfSize(int n, string filename)
        {
            var uniqueUnaryAutomatons = Generator.GetAllUniqueAutomataOfSize(n);
            using (var stream = File.Create(filename))
            {
                var serializer = new XmlSerializer(typeof(int[][]));
                serializer.Serialize(stream, uniqueUnaryAutomatons);
            }
        }
    }
}
