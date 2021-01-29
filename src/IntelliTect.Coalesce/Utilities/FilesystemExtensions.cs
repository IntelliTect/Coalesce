using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce
{
    public static class FilesystemExtensions
    {
        public static FileInfo? FindFileInAncestorDirectory(
            this DirectoryInfo directory, string fileName)
        {
            var curDirectory = directory;
            while (curDirectory != null)
            {
                FileInfo matchingFile = curDirectory.EnumerateFiles(fileName).FirstOrDefault();
                if (matchingFile != null) return matchingFile;
                curDirectory = curDirectory.Parent;
            }
            return null;
        }
    }
}
