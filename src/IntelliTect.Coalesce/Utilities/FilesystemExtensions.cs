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
                FileInfo? matchingFile = curDirectory.EnumerateFiles(fileName).FirstOrDefault();
                if (matchingFile != null) return matchingFile;
                curDirectory = curDirectory.Parent;
            }
            return null;
        }

        public static DirectoryInfo? FindDirectoryInAncestorDirectory(
            this DirectoryInfo directory, string fileName)
        {
            var curDirectory = directory;
            while (curDirectory != null)
            {
                DirectoryInfo? matchingFile = curDirectory.EnumerateDirectories(fileName).FirstOrDefault();
                if (matchingFile != null) return matchingFile;
                curDirectory = curDirectory.Parent;
            }
            return null;
        }

        public static DirectoryInfo? FindDirectoryInAncestorDirectory(
            this DirectoryInfo directory, Func<DirectoryInfo, bool> predicate)
        {
            var curDirectory = directory;
            while (curDirectory != null)
            {
                var match = curDirectory.EnumerateDirectories().FirstOrDefault(predicate);
                if (match != null) return match;
                curDirectory = curDirectory.Parent;
            }
            return null;
        }

        public static DirectoryInfo? GetDirectory(this DirectoryInfo directory, string child)
        {
            return new DirectoryInfo(Path.Combine(directory.FullName, child));
        }
    }
}
