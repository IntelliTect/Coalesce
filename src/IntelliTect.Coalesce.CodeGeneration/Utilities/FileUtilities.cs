using System;
using System.IO;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities;

public static class FileUtilities
{
    /// <summary>
    /// Compares a stream to the target file ignoring line endings.
    /// </summary>
    /// <param name="sourceStream">Stream to compare with the file.</param>
    /// <param name="origContents">File contents to compare with the stream.</param>
    /// <returns></returns>
    public static async Task<bool> HasDifferencesAsync(Stream sourceStream, string origContents)
    {
        if (string.IsNullOrEmpty(origContents)) return true;
        origContents = origContents.Replace("\r\n", "\n");

        var newContents = Task.Run(async () =>
        {
            sourceStream.Seek(0, SeekOrigin.Begin);
            string sourceString = await new StreamReader(sourceStream).ReadToEndAsync();
            sourceStream.Seek(0, SeekOrigin.Begin);
            return sourceString.Replace("\r\n", "\n");
        });
        return !string.Equals(origContents, await newContents, StringComparison.Ordinal);
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
        string f1 = File.ReadAllText(fileName1).Replace("\r\n", "\n");
        string f2 = File.ReadAllText(fileName2).Replace("\r\n", "\n");

        // Check the file sizes. If they are not the same, the files 
        // are not the same.
        if (f1.Length != f2.Length)
        {
            return true;
        }

        if (!string.Equals(f1, f2, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
