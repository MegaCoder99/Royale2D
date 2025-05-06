using System.IO;

namespace Shared;

public class FolderPath
{
    public string fullPath;
    public string folderName => Path.GetFileName(fullPath) ?? "";

    public FolderPath(string fullPath)
    {
        fullPath = SharedHelpers.NormalizePath(fullPath);
        this.fullPath = fullPath.TrimEnd('/');
    }

    public FolderPath(FolderPath basePath, string folderName)
    {
        folderName = folderName.Replace('\\', '/');
        fullPath = basePath.fullPath + "/" + folderName.TrimStart('/');
    }

    // Alternative option/alias for constructor
    public static FolderPath New(string fullPath) => new FolderPath(fullPath);

    public static FolderPath Desktop() => New(GetDesktopRawFolderPath());
    public static FilePath GetDesktopFilePath(string fileName) => Desktop().AppendFile(fileName);
    public static string GetDesktopRawFolderPath()
    {
        try
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        catch
        {
            // If for some reason we can't get the desktop folder path, return "", which will use the application folder path
            return "";
        }
    }

    public void Delete()
    {
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
    }

    public void DeleteAndRecreate()
    {
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        Directory.CreateDirectory(fullPath);
    }

    public List<FilePath> GetFiles()
    {
        List<FilePath> files = new List<FilePath>();
        if (Directory.Exists(fullPath))
        {
            foreach (string file in Directory.GetFiles(fullPath))
            {
                files.Add(new FilePath(file));
            }
        }
        return files;
    }

    public List<FilePath> GetFiles(bool recursive, params string[] filters)
    {
        var files = new List<string>();
        if (Directory.Exists(fullPath))
        {
            files = Directory.GetFiles(fullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
        }

        return files.Where(f =>
        {
            if (filters == null || filters.Length == 0) return true;
            foreach (var filter in filters)
            {
                if (f.EndsWith(filter, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }).Select(f => new FilePath(f)).ToList();
    }

    public List<FolderPath> GetFolders()
    {
        List<FolderPath> folders = new List<FolderPath>();
        foreach (string folder in Directory.GetDirectories(fullPath))
        {
            folders.Add(new FolderPath(folder));
        }
        return folders;
    }

    // fileExtension may be explicitly passed in if desired
    public FilePath AppendFile(string fileName, string fileExtension = "")
    {
        fileExtension = fileExtension == "" ? "" : "." + fileExtension;
        return new FilePath(fullPath + "/" + fileName + fileExtension);
    }

    public FolderPath AppendFolder(string folderName)
    {
        if (folderName == "") return new FolderPath(fullPath);
        return new FolderPath(fullPath + "/" + folderName);
    }

    public FilePath AppendRelativeFilePath(FilePath relativeFilePath)
    {
        return new FilePath(fullPath + "/" + relativeFilePath.fullPath);
    }

    public void CreateIfNotExists()
    {
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public bool IsEmpty()
    {
        return !Directory.EnumerateFileSystemEntries(fullPath).Any();
    }

    public bool Exists()
    {
        return Directory.Exists(fullPath);
    }

    public static FolderPath operator +(FolderPath a, string b)
    {
        throw new InvalidOperationException("Adding a string to FolderPath is not supported.");
    }

    // Similarly, disallow adding FolderPath to a string
    public static FolderPath operator +(string a, FolderPath b)
    {
        throw new InvalidOperationException("Adding MyCustomClass to a string is not supported.");
    }

    public bool EqualTo(FolderPath other)
    {
        return fullPath == other.fullPath;
    }

    public bool EqualTo(string otherFolderPath)
    {
        return EqualTo(New(otherFolderPath));
    }

    public static bool IsPathValid(string path)
    {
        try
        {
            Path.GetFullPath(path); // This will throw if the path is invalid
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void CopyTo(FolderPath destinationFolderPath)
    {
        string sourcePath = fullPath;
        string destPath = destinationFolderPath.fullPath;

        CopyFolder(sourcePath, destPath);
    }

    void CopyFolder(string sourcePath, string destPath)
    {
        // Ensure destination directory exists
        Directory.CreateDirectory(destPath);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            string destFile = Path.Combine(destPath, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        // Recursively copy all subdirectories
        foreach (var directory in Directory.GetDirectories(sourcePath))
        {
            string destDirectory = Path.Combine(destPath, Path.GetFileName(directory));
            CopyFolder(directory, destDirectory);
        }
    }
}
