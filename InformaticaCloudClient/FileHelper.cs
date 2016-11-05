using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;

namespace InformaticaCloudClient
{
    public static class FileHelper
    {
        public static void MakeCsv(List<ActivityReportRecord> source, string filePath )
        {
            /*
            MemoryStream stream = new MemoryStream();
            CsvSerializer.SerializeToStream<List<ActivityReportRecord>>(source, stream);
            stream.Seek(0,SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
            */
            var file = System.IO.File.Create(filePath);
            StreamWriter writer = new StreamWriter(file);
            writer.WriteCsv(source);
            writer.Close();
        }
    }
}
