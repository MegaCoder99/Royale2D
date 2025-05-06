using Shared;

namespace Editor;

// Each editor has two possible configs. One app-wide and top-level (Config), one per workspace (WorkspaceConfig)

public partial class Config
{
    // This is the saved workspace that is automatically opened on subsequent editor launches
    public string workspacePath = "";

    public List<string> recentWorkspaces = [];
    public Dictionary<string, string> lastFolderBrowserPaths = new();
    public Dictionary<string, string> lastFileBrowserPaths = new();

    private static Config? _main;
    public static Config main
    {
        get
        {
            if (_main == null)
            {
                FilePath configFilePath = new Config().GetConfigFilePath();
                if (configFilePath.Exists())
                {
                    _main = JsonHelpers.DeserializeJsonFile<Config>(configFilePath);
                }
                else
                {
                    _main = new Config();
                }
            }

            return _main;
        }
    }

    // Should generally only be called on application exit
    public void Save()
    {
        JsonHelpers.SerializeToJsonFile(GetConfigFilePath(), this);
    }
}

public partial class WorkspaceConfig
{
    public const string FileName = "workspace_config.json";

    public static WorkspaceConfig main;

    public static void Init(string workspacePath)
    {
        FilePath configFilePath = FolderPath.New(workspacePath).AppendFile(FileName);
        if (configFilePath.Exists())
        {
            main = JsonHelpers.DeserializeJsonFile<WorkspaceConfig>(configFilePath);
        }
        else
        {
            main = new WorkspaceConfig();
        }
    }

    public void Save(string workspacePath)
    {
        FilePath configFilePath = FolderPath.New(workspacePath).AppendFile(FileName);
        JsonHelpers.SerializeToJsonFile(configFilePath, this);
    }
}