using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public partial class State
{
    [Script("ep")]
    public void ExportPaletteData(string[] args)
    {
        var paletteData = new PaletteData(this);

        // Draw to bitmap for visualization
        List<KeyValuePair<Color, int>> orderedColorKvps = paletteData.globalColorToCount.ToList();
        orderedColorKvps.Sort((a, b) => b.Value.CompareTo(a.Value));

        BitmapDrawer bitmapDrawer = new(100, orderedColorKvps.Count * 16);
        int yPos = 0;
        foreach ((Color color, int count) in orderedColorKvps)
        {
            bitmapDrawer.DrawRect(MyRect.CreateWH(0, yPos * 16, 16, 16), color);
            bitmapDrawer.DrawText(count.ToString(), 16, (yPos + 1) * 16, Color.Black, null, 12);
            yPos++;
        }
        bitmapDrawer.SaveBitmapToDisk(FolderPath.GetDesktopFilePath("palette.png").fullPath);

        Prompt.ShowMessage("Done");
    }

    [Script("pr")]
    public void PaletteReduceTiles(string[] args)
    {
        if (HasOrphanedTileIds())
        {
            Prompt.ShowMessage("There are unused tile ids. Please remove them first.");
            return;
        }

        var tileToCount = GetTileToCount();
        var paletteData = new PaletteData(this);

        Dictionary<Tile, Tile> tilesToReplace = [];
        foreach (Tile tile in tileset.GetAllTiles())
        {
            Color[,] tileColors = tileset.GetColorsFromTileHash(tile.hash, true);
            for (int y = 0; y < tileset.tileSize; y++)
            {
                for (int x = 0; x < tileset.tileSize; x++)
                {
                    Color tileColor = tileColors[y, x];
                    Color masterColor = paletteData.colorToMasterColor[tileColor];
                    tileColors[y, x] = masterColor;
                }
            }

            string newHash = tileset.GetTileHashFromColors(tileColors);
            if (tile.hash == newHash) continue;

            Tile? replaceWithThisTile = tileset.GetFirstTileByHash(newHash);
            if (replaceWithThisTile != null)
            {
                tilesToReplace[tile] = replaceWithThisTile;
            }
        }

        BitmapDrawer bitmapDrawer = new(150, tilesToReplace.Count * 16);
        int yPos = 0;
        foreach ((Tile tileToReplace, Tile replaceWithThisTile) in tilesToReplace)
        {
            if (tileToCount[replaceWithThisTile] == 1 && tileToCount[tileToReplace] == 102)
            {
                Console.WriteLine(replaceWithThisTile.hash);
                Console.WriteLine(tileToReplace.hash);
            }

            int xPos = 0;

            if (replaceWithThisTile.hash == tileToReplace.hash)
            {
                throw new Exception("Same hash");
            }

            bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(replaceWithThisTile.hash), xPos, yPos * 16);

            xPos += 16;
            bitmapDrawer.DrawImage(tileset.GetDrawerFromTileHash(tileToReplace.hash), xPos, yPos * 16);

            xPos += 16;

            string message = $"{tileToCount[replaceWithThisTile]},{tileToCount[tileToReplace]}";
            bitmapDrawer.DrawText(message, xPos, -4 + ((yPos + 1) * 16), Color.Black, null, 16);

            yPos++;
        }
        bitmapDrawer.SaveBitmapToDisk(FolderPath.GetDesktopFilePath("palette_reduced_tiles.png").fullPath);
        Prompt.ShowMessage("Saved visualization of changes in \"palette_reduced_tiles.png\"");
    }
}
