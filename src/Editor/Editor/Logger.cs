using Shared;

namespace Editor;

public class Logger
{
    private static string loggedText = "";

    public static string LogText(string text)
    {
        loggedText += text + Environment.NewLine + Environment.NewLine;
        return text;
    }

    public static void LogException(Exception ex)
    {
        loggedText += ex.ToString() + Environment.NewLine + Environment.NewLine;
    }

    public static void SaveToDisk()
    {
        if (loggedText.Length > 0)
        {
            FilePath.New("logs.txt").WriteAllText(loggedText);
        }
    }
}
