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
            var uniqueUnaryAutomatons = Generator.EnumerateCollectionsOfNonisomorphicUnaryAutomata().Skip(n - 1).First();
            using (var stream = File.Create(filename))
            {
                var serializer = new XmlSerializer(typeof(int[][]));
                serializer.Serialize(stream, uniqueUnaryAutomatons);
            }
        }
    }
}
