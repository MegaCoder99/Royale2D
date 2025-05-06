using System.IO;

namespace Shared;

public class SpriteWorkspace : IWorkspace
{
    public const string SpriteFolderName = "sprites";
    public const string SpritesheetFolderName = "spritesheets";

    public FolderPath baseFolderPath { get; set; }
    public FolderPath spritesPath;
    public FolderPath spritesheetPath;

    private bool loaded;
    private List<SpritesheetModel> _spritesheets = [];
    private List<SpriteModel> _sprites = [];

    public List<SpritesheetModel> spritesheets
    {
        get
        {
            if (!loaded)
            {
                throw new Exception("Spritesheets not loaded. Call LoadFromDisk() first.");
            }
            return _spritesheets;
        }
    }

    public List<SpriteModel> sprites
    {
        get
        {
            if (!loaded)
            {
                throw new Exception("Sprites not loaded. Call LoadFromDisk() first.");
            }
            return _sprites;
        }
    }

    public SpriteWorkspace(string baseRawFolderPath)
    {
        baseFolderPath = new FolderPath(baseRawFolderPath);
        spritesPath = new FolderPath(baseFolderPath, SpriteFolderName);
        spritesheetPath = new FolderPath(baseFolderPath, SpritesheetFolderName);
    }

    public bool IsValid(out string errorMessage)
    {
        List<string> errorMessages = [];

        if (!spritesheetPath.Exists())
        {
            errorMessages.Add($"{SpritesheetFolderName} folder not found in workspace folder");
        }
        else if (!spritesheetPath.GetFiles().Any(f => f.ext == "png"))
        {
            errorMessages.Add($"png file not found in {SpritesheetFolderName} folder");
        }

        if (!spritesPath.Exists())
        {
            errorMessages.Add($"{SpriteFolderName} folder not found in workspace folder");
        }

        errorMessage = string.Join('\n', errorMessages);

        return errorMessage.Unset();
    }

    public void CreateFolders()
    {
        spritesPath.CreateIfNotExists();
        spritesheetPath.CreateIfNotExists();
    }

    public void RecreateFolders()
    {
        spritesPath.DeleteAndRecreate();
        spritesheetPath.DeleteAndRecreate();
    }

    public void LoadFromDisk(bool isPackaged)
    {
        _spritesheets = LoadSpritesheets();
        _sprites = LoadSprites();

        // As much as possible do any validation here, e.g. before UI loads to user after startup

        // Throw exception if a sprite references a spritesheet that doesn't exist
        if (!isPackaged)
        {
            foreach (SpriteModel sprite in _sprites)
            {
                if (!_spritesheets.Any(s => s.name == sprite.spritesheetName))
                {
                    throw new Exception($"Sprite {sprite.name} references non-existent spritesheet {sprite.spritesheetName}");
                }
            }
        }

        loaded = true;
    }

    private List<SpritesheetModel> LoadSpritesheets()
    {
        List<SpritesheetModel> spritesheets = [];
        foreach (FilePath spritesheetFile in spritesheetPath.GetFiles(true, "png"))
        {
            // For spritesheets, don't use fullPathNoExt, include extension in the name for user clarity
            string name = spritesheetFile.GetRelativeFilePath(spritesheetPath).fullPath;
            SpritesheetModel spritesheet = new(name, spritesheetFile);
            spritesheets.Add(spritesheet);
        }
        return spritesheets;
    }

    private List<SpriteModel> LoadSprites()
    {
        List<SpriteModel> sprites = [];
        foreach (FilePath spriteFile in spritesPath.GetFiles(true, "json"))
        {
            string spriteJson = File.ReadAllText(spriteFile.fullPath);
            SpriteModel sprite = JsonHelpers.DeserializeJson<SpriteModel>(spriteJson);
            string name = spriteFile.GetRelativeFilePath(spritesPath).fullPathNoExt;
            sprites.Add(sprite with { name = name });
        }
        return sprites;
    }

    public void SaveSprite(SpriteModel sprite)
    {
        FilePath filePath = new FilePath(spritesPath, sprite.name + ".json");

        // Clear out name before saving, because it is implicitly represented by file system path saved on disk
        // and having it redundantly stored in the json file can lead to confusion and errors when moving files around
        SpriteModel cleanedSprite = sprite with { name = "" };
        JsonHelpers.SerializeToJsonFile(filePath, cleanedSprite);
    }
}
