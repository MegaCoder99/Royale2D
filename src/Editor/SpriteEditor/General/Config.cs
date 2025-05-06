using Shared;

// Declare "editor" namespace since shared needs to reference it too
namespace Editor;

public partial class Config
{
    // When debugging locally, put config.json on the desktop for easier access
#if DEBUG
    public FilePath GetConfigFilePath() => FolderPath.GetDesktopFilePath("sprite_editor_config.json");
#else
    public FilePath GetConfigFilePath() => FilePath.New("config.json");
#endif

    public const string NewWorkspaceSavedPathKey = "NewWorkspaceSavedPathKey";
    public const string OpenWorkspaceSavedPathKey = "OpenWorkspaceSavedPathKey";
    public const string ImportSpritesheetSavedPathKey = "ImportSpritesheetSavedPathKey";
    public const string ExportSavedPathKey = "ExportSavedPathKey";
}

public partial class WorkspaceConfig
{
    // CONFIG BOOKMARK (DEFINITIONS)

    public string lastSpriteFilter = "";
    public string lastSpriteName = "";
    public string lastBoxTagFilter = "";
    public string lastScript = "";
}