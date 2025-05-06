#pragma warning disable CS8618
namespace Editor;

public class LaunchArgs
{
    public const string NewWorkspaceArg = "nw";
    public const string OpenWorkspaceArg = "ow";

    private static string[] startupArgs;

    public static void Init(string[] startupArgs)
    {
        LaunchArgs.startupArgs = startupArgs;
    }

    public static bool Contains(string arg)
    {
        return startupArgs.Contains(arg);
    }

    public static string GetBundledArgs(params string[] args)
    {
        return string.Join(" ", args);
    }
}
