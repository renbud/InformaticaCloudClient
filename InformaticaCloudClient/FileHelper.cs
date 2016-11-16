using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;

namespace InformaticaCloudClient
{
    public static class FileHelper
    {
        public static void MakeLogFileCsv(List<ActivityReportRecord> source, string optFilePath, string taskName )
        {
            var defaultFileName = "Log" + taskName + ".csv";
            var folderName = Path.GetDirectoryName(optFilePath);
            var specifiedFileName = Path.GetFileName(optFilePath);
            var extension = Path.GetExtension(optFilePath);

            string logFilePath;
            if (String.IsNullOrEmpty(extension))
                logFilePath = Path.Combine(folderName, specifiedFileName, defaultFileName);
            else
                logFilePath = Path.Combine(folderName, specifiedFileName);

            var file = File.Create(logFilePath);
            StreamWriter writer = new StreamWriter(file);
            writer.WriteCsv(source);
            writer.Close();
        }

        public static void WriteLogToConsole(List<ActivityReportRecord> source, string optFilePath, string taskName)
        {

            MemoryStream stream = new MemoryStream();
            CsvSerializer.SerializeToStream<List<ActivityReportRecord>>(source, stream);
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
            stream.Close();
        }

    }
}
