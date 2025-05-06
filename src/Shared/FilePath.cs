using System.Diagnostics;
using System.IO;

namespace Shared;

public class FilePath
{
    public string fullPath;
    public string fileNameWithExt => Path.GetFileName(fullPath);
    public string fileNameNoExt => Path.GetFileNameWithoutExtension(fullPath);
    public string fullPathNoExt => fullPath.Substring(0, fullPath.Length - ext.Length - 1);
    public string ext => Path.GetExtension(fullPath).ToLowerInvariant().Trim('.');

    public FilePath(string fullPath)
    {
        fullPath = SharedHelpers.NormalizePath(fullPath);
        this.fullPath = fullPath;
        if (!fullPath.Contains(".") && Debugger.IsAttached)
        {
            // File should have an extension. If it doesn't, it's likely (but not always) a mistake.
            Debugger.Break();
        }
    }

    public FilePath(FolderPath folderPath, string fileNameWithExtension)
    {
        fileNameWithExtension = fileNameWithExtension.Replace('\\', '/');
        fullPath = folderPath.fullPath + "/" + fileNameWithExtension.TrimStart('/');
    }

    // Alternative option/alias for constructor
    public static FilePath New(string fullPath) => new FilePath(fullPath);

    public bool ContainsPath(string path)
    {
        return fullPath.EndsWith("/" + path);
    }

    public static bool operator ==(FilePath a, FilePath b)
    {
        return a.fullPath == b.fullPath;
    }

    public static bool operator !=(FilePath a, FilePath b)
    {
        return a.fullPath != b.fullPath;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        if (ReferenceEquals(this, obj)) return true;
        return fullPath == ((FilePath)obj).fullPath;
    }

    public override int GetHashCode()
    {
        return fullPath.GetHashCode();
    }

    public void Delete()
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    // oldToNewConverter takes in the old file name (WITHOUT EXTENSION) and returns the new file name (AGAIN, WITHOUT EXTENSION)
    public FilePath ChangeFileName(Func<string, string> oldToNewConverter)
    {
        string newFileName = oldToNewConverter(fileNameNoExt);
        string? prefix = Path.GetDirectoryName(fullPath);
        if (prefix.IsSet()) prefix += "/";
        return new FilePath(prefix + newFileName + "." + ext);
    }

    public FilePath ChangeFileExt(string newExt)
    {
        return new FilePath(fullPathNoExt + "." + newExt);
    }

    public string GetPathAfter(FolderPath imagePath)
    {
        return fullPath.Substring(imagePath.fullPath.Length + 1);
    }

    public void AssertIsRelative()
    {
        if (fullPath.Contains(":"))
        {
            throw new Exception("Path must be relative and not absolute");
        }
        if (fullPath.StartsWith("/") || fullPath.EndsWith("/"))
        {
            throw new Exception("Path must not start or end with a slash");
        }
    }

    // Example input/output:
    // this.fullPath  = "C:/users/username/desktop/folder/test.png"
    // baseFolderPath = "C:/users/username/desktop"
    // Should return "folder/test.png"
    public FilePath GetRelativeFilePath(FolderPath baseFolderPath)
    {
        if (!fullPath.StartsWith(baseFolderPath.fullPath))
        {
            throw new Exception("Base folder path is not a parent of this file path");
        }
        return new FilePath(fullPath.Substring(baseFolderPath.fullPath.Length + 1));
    }

    public T DeserializeJson<T>()
    {
        string json = File.ReadAllText(fullPath);
        return JsonHelpers.DeserializeJson<T>(json);
    }

    // Gets full path using this folder as a base instead
    public FilePath GetPathAtFolder(FolderPath newFolderPath)
    {
        return new FilePath(newFolderPath, fileNameWithExt);
    }

    public void CreateIfNotExists(string contents)
    {
        if (!File.Exists(fullPath))
        {
            WriteAllText(contents);
        }
    }

    public void WriteAllText(string contents)
    {
        string folderPath = Path.GetDirectoryName(fullPath)!;
        if (folderPath.IsSet() && !Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        File.WriteAllText(fullPath, contents);
    }

    public bool Exists()
    {
        return File.Exists(fullPath);
    }

    public string ReadAllText()
    {
        try
        {
            return File.ReadAllText(fullPath);
        }
        catch
        {
            return "";
        }
    }

    public string GetPathAfterFolderWithoutFileName(string folderName)
    {
        if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(folderName))
        {
            return string.Empty;
        }

        // Normalize the paths to avoid case sensitivity issues.
        string normalizedFullPath = Path.GetFullPath(fullPath);
        folderName = folderName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Find the folderName within the full path.
        int folderIndex = normalizedFullPath.IndexOf($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

        if (folderIndex == -1)
        {
            return string.Empty;
        }

        // Get the path after the folderName.
        int startIndex = folderIndex + folderName.Length + 1;
        string afterFolderPath = normalizedFullPath.Substring(startIndex);

        // Remove the file name if present.
        string? directoryPart = Path.GetDirectoryName(afterFolderPath)?.Replace(Path.DirectorySeparatorChar, '/');

        return string.IsNullOrEmpty(directoryPart) ? string.Empty : directoryPart.Trim('/');
    }

    public void CopyTo(FolderPath folderPath)
    {
        File.Copy(fullPath, folderPath.AppendFile(fileNameWithExt).fullPath, true);
    }
}
