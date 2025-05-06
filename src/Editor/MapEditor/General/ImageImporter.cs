using Editor;
using Shared;
using System.Collections.Concurrent;

namespace MapEditor;

public class ImageImporter
{
    public int[,] importGrid = new int[0, 0];
    public int numberOfNewTilesAdded;
    public string fileNameNoExt = "";
    public string fullPath = "";

    // Not static so we can have additional fields "returned" like numberOfNewTilesAdded
    public ImageImporter Import(FilePath imagePath, Tileset tileset)
    {
        int TS = tileset.tileSize;
        fileNameNoExt = imagePath.fileNameNoExt;
        fullPath = imagePath.fullPath;
        int oldTileCount = tileset.idToTile.Keys.Count;

        var bitmap = BitmapHelpers.CreateBitmapFromFile(imagePath);
        int width = bitmap.Width - (bitmap.Width % TS);
        int height = bitmap.Height - (bitmap.Height % TS);
        if (bitmap.Width % TS != 0 || bitmap.Height % TS != 0)
        {
            Prompt.ShowWarning($"Image file \"{imagePath.fileNameWithExt}\" not multiple of {TS}x{TS} in size.\n\nThe import process will involve cutoff at bottom/right of image.");
        }

        var lockedBitmap = new LockedBitmap(bitmap);
        lockedBitmap.LockBits();

        importGrid = new int[height / TS, width / TS];

        ConcurrentDictionary<string, Tile?> cache = [];

        for (int y = 0; y < height; y += TS)
        {
            for (int x = 0; x < width; x += TS)
            {
                string tileHash = tileset.GetTileHash(lockedBitmap, x, y);
                Tile? newTile = tileset.GetFirstTileByHash(tileHash);
                if (newTile == null)
                {
                    newTile = tileset.AddNewTile(tileHash);
                }

                importGrid[y / TS, x / TS] = newTile.id;
            }
        }
        lockedBitmap.UnlockBits();

        int newTileCount = tileset.idToTile.Keys.Count;
        numberOfNewTilesAdded = newTileCount - oldTileCount;

        bitmap.Dispose();

        return this;
    }
}