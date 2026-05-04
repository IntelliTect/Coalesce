using System.IO;
using System.Linq;

namespace IntelliTect.Coalesce;

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

    public static DirectoryInfo? GetDirectory(this DirectoryInfo directory, string child)
    {
        return new DirectoryInfo(Path.Combine(directory.FullName, child));
    }

    public static DirectoryInfo? GetRepoRoot()
    {
        return
            // Normal usage (e.g. executing out of a /bin folder
            new DirectoryInfo(Directory.GetCurrentDirectory())
                .FindFileInAncestorDirectory("Coalesce.slnx")
                ?.Directory
        ??
            // For Live Unit Testing, which makes a copy of the whole repo elsewhere.
            new DirectoryInfo(Directory.GetCurrentDirectory())
                .FindDirectoryInAncestorDirectory("b");
    }
}
