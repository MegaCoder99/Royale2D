using Shared;

// Declare "editor" namespace since shared needs to reference it too
namespace Editor;

public partial class Config
{
    // When debugging locally, put config.json on the desktop for easier access
#if DEBUG
    public FilePath GetConfigFilePath() => FolderPath.GetDesktopFilePath("map_editor_config.json");
#else
    public FilePath GetConfigFilePath() => FilePath.New("config.json");
#endif

    public const string NewWorkspaceSavedPathKey = "NewWorkspaceSavedPathKey";
    public const string OpenWorkspaceSavedPathKey = "OpenWorkspaceSavedPathKey";
    public const string ImportInitialMapSectionSavedPathKey = "ImportInitialMapSectionSavedPathKey";
    public const string ImportInitialScratchSectionSavedPathKey = "ImportInitialScratchSectionSavedPathKey";
    public const string ImportSectionSavedPathKey = "ImportSectionSavedPathKey";
    public const string ExportSavedPathKey = "ExportSavedPathKey";
}

public partial class WorkspaceConfig
{
    // CONFIG BOOKMARK (DEFINITIONS)

    public string lastMapSection = "";
    public int lastMapSectionZoom = 1;
    public int lastMapCanvasScrollX;
    public int lastMapCanvasScrollY;
    public string lastScratchSection = "";
    public int lastScratchCanvasZoom = 1;
    public int lastScratchCanvasScrollX;
    public int lastScratchCanvasScrollY;
    public string lastMapScript = "";
    public int lastMode;
    public List<int> lastSelectedMapLayerIndices = [];

    public bool showGrid;
    public bool showEntities;
    public bool showTileHitboxes;
    public bool showTileZIndices;
    public bool showTileAnimations;
    public bool showTileClumps;
    public bool showSameTiles;
    public bool hideScratchSection;
    public bool mapCanvasMagentaBgColor;
    public bool scratchCanvasMagentaBgColor;
    public bool showUnselectedMapLayers;
}