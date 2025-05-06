using Editor;
using Shared;
using System.IO;

namespace SpriteEditor;

public class Exporter
{
    public const string DefaultExportFolder = "exported";

    private State state;
    public SpriteWorkspace workspace => state.workspace;

    public SpriteWorkspace exportWorkspace;
    public int maxImageSize = 1024;

    // If you have "ball.png" and "ball_red.png" spritesheet which are just color swaps,
    // put "ball" as key and ["red"] as value to clone all sprites using "ball", i.e. "ball.json" to also have a cloned sprite variant using "ball_red.png" called "ball_red.json"
    public Dictionary<string, List<string>> spritesheetPaletteVariants = new()
    {
        { "boomerang", ["red"] },
        { "arrow", ["silver"] }
    };

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
            string spritesheetNameNoExt = Path.GetFileNameWithoutExtension(sprite.spritesheetName);

            List<string>? matches = spritesheetPaletteVariants.GetValueOrDefault(spritesheetNameNoExt);
            if (matches != null)
            {
                foreach (string match in matches)
                {
                    SpriteModel clonedPaletteSprite = sprite.ToModel() with { name = $"{sprite.name}_{match}" };
                    allClonedSprites.Add(clonedPaletteSprite);
                }
            }
        }

        Dictionary<string, ExportedPixelRect> hashToExportedFrames = new();

        foreach (SpriteModel sprite in allClonedSprites)
        {
            foreach (FrameModel frame in sprite.frames)
            {
                string frameHash = GetFrameHash(sprite.spritesheetName, frame.rect);
                if (!hashToExportedFrames.ContainsKey(frameHash))
                {
                    BitmapDrawer drawer = new(frame.rect.w, frame.rect.h);
                    drawer.DrawImage(GetSpritesheetDrawer(sprite.spritesheetName), 0, 0, frame.rect.x1, frame.rect.y1, frame.rect.w, frame.rect.h);
                    hashToExportedFrames[frameHash] = new ExportedPixelRect(drawer);
                }

                foreach (DrawboxModel drawbox in frame.drawboxes)
                {
                    string drawboxHash = GetFrameHash(drawbox.spritesheetName, drawbox.rect);
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
            List<FrameModel> newFrames = sprite.frames.SelectList((FrameModel frame) =>
            {
                string frameHash = GetFrameHash(sprite.spritesheetName, frame.rect);
                ExportedPixelRect exportedFrame = hashToExportedFrames[frameHash];

                List<DrawboxModel> newDrawboxes = frame.drawboxes.SelectList((DrawboxModel drawbox) =>
                {
                    string drawboxHash = GetFrameHash(drawbox.spritesheetName, drawbox.rect);
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

    private string GetFrameHash(string spritesheetPath, MyRect rect)
    {
        return spritesheetPath + "_" + rect.ToString();
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
