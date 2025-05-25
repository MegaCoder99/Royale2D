using Editor;
using Shared;
using System.Drawing;
using System.IO;

namespace SpriteEditor;

public record SpritePaletteVariant
{
    public string spriteName;
    public string suffix;
    public string[] colorSwaps;

    public SpritePaletteVariant(string spriteName, string suffix, params string[] colorSwaps)
    {
        this.spriteName = spriteName;
        this.suffix = suffix;
        this.colorSwaps = colorSwaps;
    }

    public void ReplaceColors(BitmapDrawer drawer)
    {
        foreach (string colorSwap in colorSwaps)
        {
            string[] colors = colorSwap.Split(',');
            if (colors.Length != 2)
            {
                continue; // Invalid color swap format
            }

            Color oldColor = Helpers.HexStringToColor(colors[0]);
            Color newColor = Helpers.HexStringToColor(colors[1]);

            drawer.ReplaceColor(oldColor, newColor);
        }
    }
}

public class Exporter
{
    public const string DefaultExportFolder = "exported";

    private State state;
    public SpriteWorkspace workspace => state.workspace;

    public SpriteWorkspace exportWorkspace;
    public int maxImageSize = 1024;


    public List<SpritePaletteVariant> spritesheetPaletteVariants = [
        new("boomerang_throw", "red", "5068a8,b02828", "90a8e8,e07070"),
        new("stone_break", "black", "e8e8e8,7bbd94", "a0a0a0,7bbd94"),
    ];

    // You can put names of spritesheet files here (without extension) that you do not want to package as part of export. These will have their raw spritesheet files remain intact.
    public List<string> spritesheetsToNotPack =
    [
        "char",
        "char_drawbox",
        "quake_lightning",
        "quake_lightning_2",
        "woods_fog",
        "woods"
    ];

    public Exporter(State state, string exportFolderPath)
    {
        this.state = state;
        exportWorkspace = new SpriteWorkspace(exportFolderPath);
    }

    public bool SkipProcessing(string spritesheetPath)
    {
        return spritesheetsToNotPack.Any(s => s.EndsWith(Path.GetFileNameWithoutExtension(spritesheetPath)));
    }

    Drawer GetSpritesheetDrawer(string spritesheetName)
    {
        return state.spritesheets.First(s => s.name == spritesheetName).drawer;
    }

    public void Export()
    {
        if (exportWorkspace.baseFolderPath.EqualTo(Config.main.workspacePath))
        {
            Prompt.ShowError("Your export path is the same as your workspace path. This would result in your workspace files being deleted!");
            return;
        }

        exportWorkspace.RecreateFolders();

        Dictionary<SpriteModel, SpritePaletteVariant> spriteModelToPaletteVariants = new();
        List<SpriteModel> allClonedSprites = [];
        foreach (Sprite sprite in state.sprites.ToList())
        {
            if (SkipProcessing(sprite.spritesheetName))
            {
                exportWorkspace.SaveSprite(sprite.ToModel());
                continue;
            }

            SpriteModel newSprite = sprite.ToModel();
            allClonedSprites.Add(newSprite);

            List<SpritePaletteVariant> matches = spritesheetPaletteVariants.Where(spv => spv.spriteName == sprite.name).ToList();
            foreach (SpritePaletteVariant match in matches)
            {
                SpriteModel clonedPaletteSprite = newSprite with
                {
                    name = $"{newSprite.name}_{match.suffix}"
                };
                allClonedSprites.Add(clonedPaletteSprite);
                spriteModelToPaletteVariants[clonedPaletteSprite] = match;
            }
        }

        Dictionary<string, ExportedPixelRect> hashToExportedFrames = new();

        foreach (SpriteModel sprite in allClonedSprites)
        {
            SpritePaletteVariant? variant = spriteModelToPaletteVariants.GetValueOrDefault(sprite);

            foreach (FrameModel frame in sprite.frames)
            {
                string frameHash = GetFrameHash(sprite.spritesheetName, frame.rect, variant);
                if (!hashToExportedFrames.ContainsKey(frameHash))
                {
                    BitmapDrawer drawer = new(frame.rect.w, frame.rect.h);
                    drawer.DrawImage(GetSpritesheetDrawer(sprite.spritesheetName), 0, 0, frame.rect.x1, frame.rect.y1, frame.rect.w, frame.rect.h);
                    variant?.ReplaceColors(drawer);

                    // This idea is useful for getting a visualization background in editor only for frame rects
                    drawer.ReplaceColor(Color.Magenta, Color.Transparent);

                    hashToExportedFrames[frameHash] = new ExportedPixelRect(drawer);
                }

                foreach (DrawboxModel drawbox in frame.drawboxes)
                {
                    string drawboxHash = GetFrameHash(drawbox.spritesheetName, drawbox.rect, null);
                    if (!hashToExportedFrames.ContainsKey(drawboxHash))
                    {
                        BitmapDrawer drawer = new(drawbox.rect.w, drawbox.rect.h);
                        drawer.DrawImage(GetSpritesheetDrawer(drawbox.spritesheetName), 0, 0, drawbox.rect.x1, drawbox.rect.y1, drawbox.rect.w, drawbox.rect.h);
                        hashToExportedFrames[drawboxHash] = new ExportedPixelRect(drawer);
                    }
                }
            }
        }

        ImagePacker imagePacker = new(exportWorkspace.spritesheetPath, "packaged", maxImageSize);
        imagePacker.PackExportImages(hashToExportedFrames.Values.ToList(), true, false);

        foreach (SpriteModel sprite in allClonedSprites)
        {
            SpritePaletteVariant? variant = spriteModelToPaletteVariants.GetValueOrDefault(sprite);

            List<FrameModel> newFrames = sprite.frames.SelectList((FrameModel frame) =>
            {
                string frameHash = GetFrameHash(sprite.spritesheetName, frame.rect, variant);
                ExportedPixelRect exportedFrame = hashToExportedFrames[frameHash];

                List<DrawboxModel> newDrawboxes = frame.drawboxes.SelectList((DrawboxModel drawbox) =>
                {
                    string drawboxHash = GetFrameHash(drawbox.spritesheetName, drawbox.rect, variant);
                    ExportedPixelRect exportedDrawbox = hashToExportedFrames[drawboxHash];

                    return drawbox with
                    {
                        rect = exportedDrawbox.newRect!.Value,
                        spritesheetName = imagePacker.GetExportImageFileName(exportedDrawbox.newSpritesheetNum)
                    };
                });

                return frame with
                {
                    rect = exportedFrame.newRect!.Value,
                    spritesheetName = imagePacker.GetExportImageFileName(exportedFrame.newSpritesheetNum),
                    drawboxes = newDrawboxes
                };
            });

            // Save the sprite without spritesheet name since we are saving the spritesheet name in the frame
            exportWorkspace.SaveSprite(sprite with { spritesheetName = "", frames = newFrames });
        }

        // Directly copy over the image files not to be processed
        foreach (FilePath spritesheetFullPath in state.spritesheets.Select(s => s.filePath))
        {
            if (SkipProcessing(spritesheetFullPath.fullPath))
            {
                CopyFile(spritesheetFullPath.fullPath, spritesheetFullPath.GetPathAtFolder(exportWorkspace.spritesheetPath).fullPath);
            }
        }
    }

    private string GetFrameHash(string spritesheetPath, MyRect rect, SpritePaletteVariant? variant)
    {
        return spritesheetPath + "_" + rect.ToString() + "_" + (variant?.suffix ?? "");
    }

    private static void CopyFile(string oldPath, string newPath)
    {
        if (File.Exists(newPath))
        {
            File.Delete(newPath);
        }
        File.Copy(oldPath, newPath);
    }
}
