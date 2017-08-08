using System;
using System.IO;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities
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
                    //Console.WriteLine($"Change to {filename}");
                }
                return result;
            }
            return false;
        }

        public static bool HasDifferences(string fileName1, string fileName2)
        {
            // Determine if the same file was referenced two times.
            if (fileName1 == fileName2)
            {
                return false;
            }

            if (!File.Exists(fileName1))
            {
                return false;
            }

            if (!File.Exists(fileName2))
            {
                return false;
            }

            // Open the two files.
            string f1 = File.ReadAllText(fileName1).Replace("\r", "").Replace("\n", "");
            string f2 = File.ReadAllText(fileName2).Replace("\r", "").Replace("\n", "");

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (f1.Length != f2.Length)
            {
                return true;
            }

            if (!string.Equals(f1, f2, StringComparison.InvariantCulture))
            {
                return true;
            }

            return false;
        }
    }
}
