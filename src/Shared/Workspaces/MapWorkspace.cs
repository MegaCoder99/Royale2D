namespace Shared;

public class MapWorkspace : IWorkspace
{
    public const string TilesetFolderName = "tileset";
    public const string TilesetFileName = "tileset.json";
    public const string TileAnimationFileName = "tile_animations.json";
    public const string TileClumpFileName = "tile_clumps.json";
    public const string MapSectionFolderName = "map_sections";
    public const string ScratchSectionFolderName = "scratch_sections";
    public const string MapImageFoldername = "images";
    public const string MinimapFolderName = "minimaps";
    public const string MinimapFileName = "minimap.json";
    public const string MinimapImageFileName = "minimap.png";
    public const string MinimapSmallFileName = "minimap_small.json";
    public const string MinimapSmallImageFileName = "minimap_small.png";

    public FolderPath baseFolderPath { get; set; }
    public FolderPath tilesetFolderPath;
    public FilePath tilesetFilePath;
    public FilePath tileAnimationFilePath;
    public FilePath tileClumpFilePath;
    public FolderPath mapSectionFolderPath;
    public FolderPath scratchSectionFolderPath;
    public FolderPath mapImageFolderPath;
    public FolderPath minimapFolderPath;
    public FilePath minimapFilePath;
    public FilePath minimapImageFilePath;
    public FilePath minimapSmallFilePath;
    public FilePath minimapSmallImageFilePath;

    private bool loaded;
    private List<MapSectionModel> _mapSections = [];
    private List<MapSectionModel> _scratchSections = [];
    private Dictionary<int, Tile> _tileset = [];
    private List<TileAnimationModel> _tileAnimations = [];
    private List<TileClumpModel> _tileClumps = [];

    public List<MapSectionModel> mapSections => GetWithGuard(() => _mapSections, nameof(mapSections));
    public List<MapSectionModel> scratchSections => GetWithGuard(() => _scratchSections, nameof(scratchSections));
    public Dictionary<int, Tile> tileset => GetWithGuard(() => _tileset, nameof(tileset));
    public List<TileAnimationModel> tileAnimations => GetWithGuard(() => _tileAnimations, nameof(tileAnimations));
    public List<TileClumpModel> tileClumps => GetWithGuard(() => _tileClumps, nameof(tileClumps));

    public MapWorkspace(string baseFolderRawPath)
    {
        baseFolderPath = new FolderPath(baseFolderRawPath);
        tilesetFolderPath = new FolderPath(baseFolderPath, TilesetFolderName);
        tilesetFilePath = new FilePath(tilesetFolderPath, TilesetFileName);
        tileAnimationFilePath = new FilePath(tilesetFolderPath, TileAnimationFileName);
        tileClumpFilePath = new FilePath(tilesetFolderPath, TileClumpFileName);
        mapSectionFolderPath = new FolderPath(baseFolderPath, MapSectionFolderName);
        scratchSectionFolderPath = new FolderPath(baseFolderPath, ScratchSectionFolderName);
        mapImageFolderPath = new FolderPath(baseFolderPath, MapImageFoldername);
        minimapFolderPath = new FolderPath(baseFolderPath, MinimapFolderName);
        minimapFilePath = new FilePath(minimapFolderPath, MinimapFileName);
        minimapImageFilePath = new FilePath(minimapFolderPath, MinimapImageFileName);
        minimapSmallFilePath = new FilePath(minimapFolderPath, MinimapSmallFileName);
        minimapSmallImageFilePath = new FilePath(minimapFolderPath, MinimapSmallImageFileName);
    }

    public string GetMapSectionImageFileName(string mapSectionName, int layerIndex, int i, int j)
    {
        // IMPROVE not having separator will limit future map sizes
        // VALIDATE that layerIndex,i,j don't go above 9
        return $"{mapSectionName}.{layerIndex}{i}{j}.png";
    }

    public bool IsValid(out string errorMessage)
    {
        List<string> errorMessages = [];
        
        if (!mapSectionFolderPath.Exists())
        {
            errorMessages.Add($"{MapSectionFolderName} folder not found in workspace folder");
        }

        if (!scratchSectionFolderPath.Exists())
        {
            errorMessages.Add($"{ScratchSectionFolderName} folder not found in workspace folder");
        }

        if (!tilesetFolderPath.Exists())
        {
            errorMessages.Add($"{TilesetFolderName} folder not found in workspace folder");
        }
        else if (!tilesetFilePath.Exists())
        {
            errorMessages.Add($"{TilesetFileName} file not found in {TilesetFolderName} folder");
        }
        else if (!tileAnimationFilePath.Exists())
        {
            errorMessages.Add($"{TileAnimationFileName} file not found in {TilesetFolderName} folder");
        }
        else if (!tileClumpFilePath.Exists())
        {
            errorMessages.Add($"{TileClumpFileName} file not found in {TilesetFolderName} folder");
        }

        errorMessage = string.Join('\n', errorMessages);

        return errorMessage.Unset();
    }

    private T GetWithGuard<T>(Func<T> getter, string name)
    {
        if (!loaded)
        {
            throw new Exception($"{name} not loaded. Call LoadFromDisk() first.");
        }
        return getter();
    }

    #region file IO
    public void CreateFolders()
    {
        tilesetFolderPath.CreateIfNotExists();
        mapSectionFolderPath.CreateIfNotExists();
        scratchSectionFolderPath.CreateIfNotExists();
        minimapFolderPath.CreateIfNotExists();
    }

    public void RecreateFolders(bool includeScratch)
    {
        tilesetFolderPath.DeleteAndRecreate();
        mapSectionFolderPath.DeleteAndRecreate();
        minimapFolderPath.DeleteAndRecreate();
        if (includeScratch) scratchSectionFolderPath.DeleteAndRecreate();
    }

    public void DeleteFolders()
    {
        tilesetFolderPath.Delete();
        mapSectionFolderPath.Delete();
        scratchSectionFolderPath.Delete();
        minimapFolderPath.Delete();
    }

    public void CreateFoldersAndFiles()
    {
        CreateFolders();
        tileAnimationFilePath.CreateIfNotExists("[]");
        tileClumpFilePath.CreateIfNotExists("[]");
    }
    #endregion

    #region load

    public void LoadFromDisk(bool isPackaged)
    {
        _mapSections = LoadMapSections();
        _scratchSections = LoadScratchSections();
        _tileset = LoadTileset();
        _tileAnimations = LoadTileAnimations();
        _tileClumps = LoadTileClumps();

        // As much as possible do any validation here, e.g. before UI loads to user after startup

        loaded = true;
    }

    private List<MapSectionModel> LoadMapSections()
    {
        var mapSections = new List<MapSectionModel>();
        foreach (FilePath filePath in mapSectionFolderPath.GetFiles(true, "json"))
        {
            MapSectionModel mapSection = JsonHelpers.DeserializeJsonFile<MapSectionModel>(filePath);
            string name = filePath.GetRelativeFilePath(mapSectionFolderPath).fullPathNoExt;
            mapSections.Add(mapSection with { name = name });
        }
        return mapSections;
    }

    private List<MapSectionModel> LoadScratchSections()
    {
        var mapSections = new List<MapSectionModel>();
        foreach (FilePath filePath in scratchSectionFolderPath.GetFiles(true, "json"))
        {
            var mapSection = JsonHelpers.DeserializeJsonFile<MapSectionModel>(filePath);
            string name = filePath.GetRelativeFilePath(scratchSectionFolderPath).fullPathNoExt;
            mapSections.Add(mapSection with { name = name, isScratch = true });
        }
        return mapSections;
    }

    private Dictionary<int, Tile> LoadTileset()
    {
        return JsonHelpers.DeserializeJson<Dictionary<int, Tile>>(tilesetFilePath.ReadAllText());
    }

    private List<TileAnimationModel> LoadTileAnimations()
    {
        return JsonHelpers.DeserializeJson<List<TileAnimationModel>>(tileAnimationFilePath.ReadAllText());
    }

    private List<TileClumpModel> LoadTileClumps()
    {
        return JsonHelpers.DeserializeJson<List<TileClumpModel>>(tileClumpFilePath.ReadAllText());
    }
    #endregion

    #region save
    public void SaveTileAnimations(List<TileAnimationModel> tileAnimations)
    {
        JsonHelpers.SerializeToJsonFile(tileAnimationFilePath, tileAnimations);
    }

    public void SaveTileClumps(List<TileClumpModel> tileClumps)
    {
        JsonHelpers.SerializeToJsonFile(tileClumpFilePath, tileClumps);
    }

    public void SaveTileset(Dictionary<int, Tile> idToTiles)
    {
        JsonHelpers.SerializeToJsonFile(tilesetFilePath, idToTiles);
    }

    public void SaveMapSection(MapSectionModel mapSection)
    {
        FilePath filePath;
        if (mapSection.isScratch == true)
        {
            filePath = new FilePath(scratchSectionFolderPath, mapSection.name + ".json");
        }
        else
        {
            filePath = new FilePath(mapSectionFolderPath, mapSection.name + ".json");
        }

        // Clear out name and isScratch before saving, because these are implicitly represented by their file system path saved on disk
        // and storing them as fields on disk again would be redundant and make moving/renaming section JSONs harder/more confusing
        MapSectionModel cleanedMapSection = mapSection with { name = "", isScratch = null };
        JsonHelpers.SerializeToJsonFile(filePath, cleanedMapSection);
    }
    #endregion
}