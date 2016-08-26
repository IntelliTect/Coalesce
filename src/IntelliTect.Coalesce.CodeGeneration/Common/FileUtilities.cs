using System;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Common
{
    public static class FileUtilities
    {
        /// <summary>
        /// Compares a stream to the target file ignoring line endings.
        /// </summary>
        /// <param name="sourceStream">Stream to compare with the file.</param>
        /// <param name="filename">File to compare with the stream.</param>
        /// <returns></returns>
        public static bool HasDifferences(Stream sourceStream, string filename)
        {
            if (File.Exists(filename))
            {
                string origString;
                using (var stream = File.OpenRead(filename))
                {
                    origString = new StreamReader(stream).ReadToEnd();
                }
                sourceStream.Seek(0, SeekOrigin.Begin);
                string sourceString = new StreamReader(sourceStream).ReadToEnd();
                origString = origString.Replace("\n", Environment.NewLine).Replace("\r\r", "\r");
                sourceString = sourceString.Replace("\n", Environment.NewLine).Replace("\r\r", "\r");
                var result = sourceString != origString;
                if (result)
                {
                    Console.WriteLine($"Change to {filename}");
                }
                return result;
            }
            return false;

        }
    }
}
