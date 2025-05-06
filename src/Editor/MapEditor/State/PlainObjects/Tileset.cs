using Editor;
using Shared;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MapEditor;

public class Tileset
{
    public const int MinTileSize = 4;
    // Can increase if needed, but not really designed for large tile sizes. Any larger and tileset.json file will get really big, hashing functions get slow, etc.
    public const int MaxTileSize = 32;

    public int _maxId = -1;

    public Dictionary<int, Tile> idToTile = new Dictionary<int, Tile>();

    public int tileSize;
    public int TS => tileSize;

    // These caches should never need to be invalidated because a tile hash representation of a tile will never change
    // The only potential problem is these getting too big and causing memory issues, but that doesn't seem like a concern at the moment
    public Dictionary<string, Drawer> cachedDrawersFromTileHash = [];
    public ConcurrentDictionary<string, Color[,]> cachedColorsFromTileHash = [];

    public Tileset(List<Tile> tiles)
    {
        // Tile size is implicitly derivable from the first tile's hash
        this.tileSize = GetTileSizeFromHash(tiles[0].hash);
        PopulateDicts(tiles);
    }

    public Tileset(int tileSize)
    {
        this.tileSize = tileSize;
        PopulateDicts([CreateTransparent()]);
    }

    public void PopulateDicts(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            idToTile[tile.id] = tile;
        }

        foreach (var tile in tiles)
        {
            if (tile.id > _maxId)
            {
                _maxId = tile.id;
            }
        }
    }

    // PERF this is a perf issue due to no longer using hash set. Not noticable right now but could be a problem in future
    public Tile? GetFirstTileByHash(string tileHash)
    {
        return idToTile.Values.FirstOrDefault(t => t.hash == tileHash);
    }

    public Tile? GetClosestTileByHash(string tileHash, ConcurrentDictionary<string, Tile?> cache)
    {
        if (cache.TryGetValue(tileHash, out Tile? cachedTile))
        {
            return cachedTile;
        }

        Tile? first = idToTile.Values.MinBy(tile => GetTileHashSimilarity(tile.hash, tileHash, 16));

        if (first != null && GetTileHashSimilarity(first.hash, tileHash, 16) < 1f)
        {
            first = null;
        }

        cache[tileHash] = first;

        return first;
    }

    public float GetTileHashSimilarity(string tileHash1, string tileHash2, int colorDiffThreshold)
    {
        Color[,] tile1Colors = GetColorsFromTileHash(tileHash1, false);
        Color[,] tile2Colors = GetColorsFromTileHash(tileHash2, false);
        return new TileSimilarity(TS).ComputeTileSimilarityByPattern(tile1Colors, tile2Colors, colorDiffThreshold);
    }

    public bool IsSimilar(Tile tile1, Tile tile2, float similarityTheshold, int colorDiffThreshold)
    {
        bool result = GetTileHashSimilarity(tile1.hash, tile2.hash, colorDiffThreshold) >= similarityTheshold;
        return result;
    }

    public Tile AddNewTile(string tileHash, Action<Action, Action>? undoRedoCallback = null)
    {
        int newTileId = ++_maxId;

        var undoAction = () =>
        {
            idToTile.Remove(newTileId);
            _maxId--;
        };

        var redoAction = () =>
        {
            var newTile = new Tile();
            newTile.hash = tileHash;
            newTile.id = newTileId;
            idToTile.Add(newTile.id, newTile);
        };

        redoAction.Invoke();

        undoRedoCallback?.Invoke(undoAction, redoAction);

        return idToTile[newTileId];
    }

    public Dictionary<int, int> ShiftTileIds()
    {
        var sortedKeys = idToTile.Keys.OrderBy(k => k).ToList();
        Dictionary<int, int> remap = new Dictionary<int, int>();
        Dictionary<int, Tile> newIdToTile = new Dictionary<int, Tile>();

        int newId = 0;
        foreach (int oldId in sortedKeys)
        {
            if (oldId != newId)
            {
                remap[oldId] = newId;
                idToTile[oldId].id = newId;
            }
            else
            {
                remap[oldId] = oldId;
            }

            newIdToTile[newId] = idToTile[oldId];
            newId++;
        }

        idToTile = newIdToTile;

        return remap;
    }

    public HashSet<DirtyFlag> ReplaceTileForeignKeyRefs(Dictionary<int, int> replacedTileIds, State mapEditorState)
    {
        HashSet<DirtyFlag> dirtyFlags = [];
        foreach (Tile tile in GetAllTiles())
        {
            if (tile.tileAboveId != null && replacedTileIds.TryGetValue(tile.tileAboveId.Value, out int newTileId))
            {
                tile.tileAboveId = newTileId;
                dirtyFlags.Add(DirtyFlag.Tile);
            }
        }

        foreach (TileAnimation tileAnimation in mapEditorState.tileAnimationSC.tileAnimations)
        {
            for (int i = 0; i < tileAnimation.tileIds.Count; i++)
            {
                if (replacedTileIds.TryGetValue(tileAnimation.tileIds[i], out int newTileId))
                {
                    tileAnimation.tileIds[i] = newTileId;
                    dirtyFlags.Add(DirtyFlag.TileAnimation);
                }
            }
        }

        foreach (TileClump tileClump in mapEditorState.tileClumpSC.tileClumps)
        {
            for (int i = 0; i < tileClump.tileIds.GetLength(0); i++)
            {
                for (int j = 0; j < tileClump.tileIds.GetLength(1); j++)
                {
                    if (replacedTileIds.TryGetValue(tileClump.tileIds[i, j], out int newTileId))
                    {
                        tileClump.tileIds[i, j] = newTileId;
                        dirtyFlags.Add(DirtyFlag.TileClump);
                    }
                }
            }
        }

        return dirtyFlags;
    }

    // "Removable" means tile id not referenced in another data model. Analogy is no SQL "foreign keys" to it
    // Some places can still remove these tile ids as long as they replace them with another tile id and call ReplaceTileForeignKeyRefs above
    // Batched for perf, though it does make code uglier. Maybe see if we can cache things somehow to avoid this
    public (HashSet<int> idsThatCanBeRemoved, HashSet<int> idsThatCantBeRemoved, Dictionary<int, string> idToCantRemoveError) GetRemovableTiles(
        HashSet<int> tileIdsToRemove, State mapEditorState)
    {
        HashSet<int> idsThatCanBeRemoved = [];
        HashSet<int> idsThatCantBeRemoved = [];
        Dictionary<int, string> idToCantRemoveError = [];

        HashSet<int> usedTileAboveIds = [];
        foreach (Tile tile in GetAllTiles())
        {
            if (tile.tileAboveId != null)
            {
                usedTileAboveIds.Add(tile.tileAboveId.Value);
            }
        }

        HashSet<int> tileIdsUsedInAnims = new();
        foreach (TileAnimation tileAnimation in mapEditorState.tileAnimationSC.tileAnimations)
        {
            foreach (int tileId in tileAnimation.tileIds)
            {
                tileIdsUsedInAnims.Add(tileId);
            }
        }

        HashSet<int> tileIdsUsedInClumps = new();
        foreach (TileClump tileClump in mapEditorState.tileClumpSC.tileClumps)
        {
            foreach (int tileId in tileClump.tileIds.ToList())
            {
                tileIdsUsedInClumps.Add(tileId);
            }
        }

        foreach (int tileId in tileIdsToRemove)
        {
            // IMPROVE export to log file?
            List<string> reasons = new List<string>();
            if (usedTileAboveIds.Contains(tileId)) reasons.Add("used in 'usedTileAboveIds'");
            if (tileIdsUsedInAnims.Contains(tileId)) reasons.Add("used in 'tileIdsUsedInAnims'");
            if (tileIdsUsedInClumps.Contains(tileId)) reasons.Add("used in 'tileIdsUsedInClumps'");
            if (reasons.Count > 0)
            {
                idsThatCantBeRemoved.Add(tileId);
                idToCantRemoveError[tileId] = string.Join(", ", reasons);
                //Console.WriteLine($"Tile ID {tileId} cannot be removed because: {string.Join(", ", reasons)}");
            }
            else
            {
                idsThatCanBeRemoved.Add(tileId);
            }
        }

        /*
        if (couldNotRemoveIds.Count > 0)
        {
            Prompt.ShowMessage("Could not remove " + couldNotRemoveIds.Count + " tiles since they are referenced in other data models.");
        }
        */

        return (idsThatCanBeRemoved, idsThatCantBeRemoved, idToCantRemoveError);
    }

    public bool CanRemoveTile(int tileId, State mapEditorState, out string? removalError)
    {
        var removableTiles = GetRemovableTiles([tileId], mapEditorState);

        if (removableTiles.idToCantRemoveError.TryGetValue(tileId, out removalError))
        {
            return false;
        }

        return removableTiles.idsThatCanBeRemoved.Contains(tileId);
    }

    public void ReplaceTileId(int oldTileId, int newTileId, Action<Action, Action>? undoRedoCallback = null)
    {
        if (!idToTile.ContainsKey(oldTileId) || !idToTile.ContainsKey(newTileId)) return;

        Tile oldTile = idToTile[oldTileId];
        Tile newTile = idToTile[newTileId];

        var undoAction = () =>
        {
            idToTile.Add(oldTile.id, oldTile);
            idToTile.Remove(newTile.id);
        };

        var redoAction = () =>
        {
            idToTile.Add(newTile.id, newTile);
            idToTile.Remove(oldTile.id);
        };

        redoAction.Invoke();

        undoRedoCallback?.Invoke(undoAction, redoAction);
    }

    // Before calling this, be sure to check references to the tile id
    public void RemoveTile(int tileId, Action<Action, Action>? undoRedoCallback = null)
    {
        if (!idToTile.ContainsKey(tileId)) return;

        Tile tile = idToTile[tileId];

        var undoAction = () =>
        {
            idToTile.Add(tileId, tile);
        };

        var redoAction = () =>
        {
            idToTile.Remove(tileId);
        };

        redoAction.Invoke();

        undoRedoCallback?.Invoke(undoAction, redoAction);
    }

    // NOTE: newTiles must be a list of cloned Tile objects with the same id/hash preserved, and this is just changing its properties
    public bool ChangeTile(List<Tile> newTiles, Action<Action, Action>? undoRedoCallback = null)
    {
        if (newTiles.Count == 0) return false;

        List<Tile> oldTiles = new List<Tile>();
        foreach (Tile tile in newTiles)
        {
            if (idToTile.ContainsKey(tile.id))
            {
                oldTiles.Add(GetTileById(tile.id));
            }
        }

        var redoAction = () =>
        {
            foreach (Tile tile in newTiles)
            {
                idToTile[tile.id] = tile;
            }
        };

        var undoAction = () =>
        {
            foreach (Tile tile in oldTiles)
            {
                idToTile[tile.id] = tile;
            }
        };

        redoAction.Invoke();

        undoRedoCallback?.Invoke(undoAction, redoAction);

        return true;
    }

    public Tile GetTileById(int tileId)
    {
        return idToTile[tileId];
    }

    public Tile GetClonedTileById(int tileId)
    {
        return Helpers.DeepCloneBinary(idToTile[tileId]);
    }

    public List<Tile> GetAllTiles()
    {
        return idToTile.Values.ToList();
    }

    public Tile CreateTransparent()
    {
        Color[,] colorGrid = new Color[TS, TS];
        for (int i = 0; i < TS; i++)
        {
            for (int j = 0; j < TS; j++)
            {
                colorGrid[i, j] = Color.Transparent;
            }
        }
        return new Tile
        {
            id = Tile.TransparentTileId,
            hash = GetTileHashFromColors(colorGrid),
        };
    }

    public Tile FlipHorizontal(Tile tile, TilesetSC tilesetSC, out bool isNew)
    {
        Color[,] colorGrid = GetColorsFromTileHash(tile.hash, true);
        for (int i = 0; i < colorGrid.GetLength(0); i++)
        {
            for (int j = 0; j < colorGrid.GetLength(1) / 2; j++)
            {
                var temp = colorGrid[i, j];
                colorGrid[i, j] = colorGrid[i, colorGrid.GetLength(1) - 1 - j];
                colorGrid[i, colorGrid.GetLength(1) - 1 - j] = temp;
            }
        }
        string newHash = GetTileHashFromColors(colorGrid);
        Tile? existingTile = GetFirstTileByHash(newHash);
        if (existingTile != null)
        {
            isNew = false;
            return existingTile;
        }
        else
        {
            isNew = true;
            return tilesetSC.AddNewTile(newHash);
        }
    }

    public Tile FlipVertical(Tile tile, TilesetSC tilesetSC, out bool isNew)
    {
        Color[,] colorGrid = GetColorsFromTileHash(tile.hash, true);
        for (int i = 0; i < colorGrid.GetLength(0) / 2; i++)
        {
            for (int j = 0; j < colorGrid.GetLength(1); j++)
            {
                var temp = colorGrid[i, j];
                colorGrid[i, j] = colorGrid[colorGrid.GetLength(0) - 1 - i, j];
                colorGrid[colorGrid.GetLength(0) - 1 - i, j] = temp;
            }
        }
        string newHash = GetTileHashFromColors(colorGrid);
        Tile? existingTile = GetFirstTileByHash(newHash);
        if (existingTile != null)
        {
            isNew = false;
            return existingTile;
        }
        else
        {
            isNew = true;
            return tilesetSC.AddNewTile(newHash);
        }
    }

    #region hashing
    public string GetTileHash(LockedBitmap image, int xArg, int yArg)
    {
        var hashes = new List<string>();
        var indexStr = "";

        for (int y = yArg; y < yArg + TS; y++)
        {
            for (int x = xArg; x < xArg + TS; x++)
            {
                Color col1 = image.GetPixel(x, y);
                if (col1.A == 0)
                {
                    col1 = Color.Transparent;
                }

                var bytes = new List<byte> { col1.R, col1.G, col1.B, col1.A };
                var hash = Convert.ToBase64String(bytes.ToArray());
                if (!hashes.Contains(hash))
                {
                    hashes.Add(hash);
                }
                int index = hashes.IndexOf(hash);
                indexStr += GetSingleCharIndex(index);
            }
        }

        var joinedHashes = string.Join(" ", hashes);
        return joinedHashes + "|" + indexStr;
    }

    public string GetTileHashFromColors(Color[,] colorGrid)
    {
        var hashes = new List<string>();
        var indexStr = "";

        for (int y = 0; y < TS; y++)
        {
            for (int x = 0; x < TS; x++)
            {
                Color col1 = colorGrid[y, x];
                if (col1.A == 0) col1 = Color.Transparent;

                var bytes = new List<byte> { col1.R, col1.G, col1.B, col1.A };
                var hash = Convert.ToBase64String(bytes.ToArray());
                if (!hashes.Contains(hash))
                {
                    hashes.Add(hash);
                }
                int index = hashes.IndexOf(hash);
                indexStr += GetSingleCharIndex(index);
            }
        }

        var joinedHashes = string.Join(" ", hashes);
        return joinedHashes + "|" + indexStr;
    }

    public Drawer GetDrawerFromTileHash(string hash)
    {
        if (cachedDrawersFromTileHash.ContainsKey(hash))
        {
            return cachedDrawersFromTileHash[hash];
        }

        Color[,] colors = GetColorsFromTileHash(hash, false);
        var drawer = new BitmapDrawer(TS, TS);

        int bytesPerPixel = 4;
        byte[] pixelData = new byte[TS * TS * bytesPerPixel];

        for (int i = 0; i < TS; i++)
        {
            for (int j = 0; j < TS; j++)
            {
                Color color = colors[i, j];
                byte[] argb = BitConverter.GetBytes(color.ToArgb());

                if (argb[3] == 0) continue; // Skip fully transparent pixels

                int index = (i * TS + j) * bytesPerPixel;
                pixelData[index] = argb[0];
                pixelData[index + 1] = argb[1];
                pixelData[index + 2] = argb[2];
                pixelData[index + 3] = argb[3];
            }
        }

        IntPtr pixelsPtr = drawer.skBitmap.GetPixels();
        if (pixelsPtr != IntPtr.Zero)
        {
            Marshal.Copy(pixelData, 0, pixelsPtr, pixelData.Length);
        }

        cachedDrawersFromTileHash[hash] = drawer;

        return drawer;
    }

    // Passing in clone returns a shallow copy. Many cases need this (if they write the color grid returned), but others (that just read it) don't, should pass false for perf
    // The reason we need to return a clone in many cases in the first place is because of the caching.
    // If we don't, we can inadvertently modify common cache from callers which is bad
    public Color[,] GetColorsFromTileHash(string hash, bool clone)
    {
        if (cachedColorsFromTileHash.ContainsKey(hash))
        {
            var cachedColors = cachedColorsFromTileHash[hash];
            if (clone) return (Color[,])cachedColors.Clone();
            return cachedColors;
        }

        string first = hash.Split("|")[0];
        string second = hash.Split("|")[1];
        List<Color> colorPool = GetColorPoolFromTileHashPrefix(first);

        var retColors = new Color[TS, TS];
        int counter = 0;
        int currentRow = 0;
        foreach (char c in second)
        {
            int charIndex = GetCharIndex(c);
            retColors[currentRow, counter] = colorPool[charIndex];
            counter++;
            if (counter >= TS)
            {
                counter = 0;
                currentRow++;
            }
        }

        cachedColorsFromTileHash[hash] = retColors;

        if (clone) return (Color[,])retColors.Clone();

        return retColors;
    }

    public List<Color> GetColorPoolFromTileHashPrefix(string hashPrefix)
    {
        var colorStrings = hashPrefix.Split(" ");
        var colorPool = new List<Color>();
        foreach (string colorString in colorStrings)
        {
            byte[] bytes = Convert.FromBase64String(colorString);
            colorPool.Add(Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]));
        }
        return colorPool;
    }

    public int GetTileSizeFromHash(string hash)
    {
        string second = hash.Split("|")[1];
        double sqrt = Math.Sqrt(second.Length);
        if (sqrt % 1 != 0)
        {
            throw new InvalidOperationException($"{sqrt} is not a perfect square.");
        }
        return (int)sqrt;
    }

    public string GetSingleCharIndex(int index)
    {
        if (index < 10) return index.ToString();
        if (index == 10) return "a";
        if (index == 11) return "b";
        if (index == 12) return "c";
        if (index == 13) return "d";
        if (index == 14) return "e";
        if (index == 15) return "f";
        if (index == 16) return "g";
        if (index == 17) return "h";
        if (index == 18) return "i";
        if (index == 19) return "j";
        if (index == 20) return "k";
        if (index == 21) return "l";
        if (index == 22) return "m";
        if (index == 23) return "n";
        if (index == 24) return "o";
        if (index == 25) return "p";
        if (index == 26) return "q";
        if (index == 27) return "r";
        if (index == 28) return "s";
        if (index == 29) return "t";
        if (index == 30) return "u";
        if (index == 31) return "v";
        if (index == 32) return "w";
        if (index == 33) return "x";
        if (index == 34) return "y";
        if (index == 35) return "z";
        if (index == 36) return "A";
        if (index == 37) return "B";
        if (index == 38) return "C";
        if (index == 39) return "D";
        if (index == 40) return "E";
        if (index == 41) return "F";
        if (index == 42) return "G";
        if (index == 43) return "H";
        if (index == 44) return "I";
        if (index == 45) return "J";
        if (index == 46) return "K";
        if (index == 47) return "L";
        if (index == 48) return "M";
        if (index == 49) return "N";
        if (index == 50) return "O";
        if (index == 51) return "P";
        if (index == 52) return "Q";
        if (index == 53) return "R";
        if (index == 54) return "S";
        if (index == 55) return "T";
        if (index == 56) return "U";
        if (index == 57) return "V";
        if (index == 58) return "W";
        if (index == 59) return "X";
        if (index == 60) return "Y";
        if (index == 61) return "Z";
        throw new Exception("Too many colors in a tile: " + index);
    }

    public int GetCharIndex(char character)
    {
        if (int.TryParse(character.ToString(), out int index) && index >= 0 && index < 10)
        {
            return index;
        }

        if (character >= 'a' && character <= 'z')
        {
            return character - 'a' + 10;
        }

        if (character >= 'A' && character <= 'Z')
        {
            return character - 'A' + 36;
        }

        throw new ArgumentException("Invalid character: " + character);
    }
    #endregion

    #region drawing

    public BitmapDrawer CreateDrawerAndDrawTileGrid(int[,] tileGrid)
    {
        BitmapDrawer bitmapDrawer = new(tileGrid.GetLength(1) * tileSize, tileGrid.GetLength(0) * tileSize);
        RedrawEntireTileBitmap(bitmapDrawer, tileGrid);
        return bitmapDrawer;
    }

    public void RedrawEntireTileBitmap(BitmapDrawer drawer, int[,] tileGrid)
    {
        int width = drawer.width;
        int height = drawer.height;
        int bytesPerPixel = 4;

        byte[] pixelData = new byte[width * height * bytesPerPixel];

        for (int i = 0; i < tileGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tileGrid.GetLength(1); j++)
            {
                int tileId = tileGrid[i, j];
                Tile tile = GetTileById(tileId);
                if (tile.encodedBytes == null)
                {
                    Color[,] pixels = GetColorsFromTileHash(tile.hash, false);
                    tile.encodedBytes = GetBytes(pixels, TS);
                }
                byte[] currentBytes = tile.encodedBytes;
                int iTimes = i * TS;
                int jTimes = j * TS;
                // unsafe for performance since we are drawing huge amounts of tiles
                unsafe
                {
                    fixed (byte* pPixelData = pixelData, pCurrentBytes = currentBytes)
                    {
                        for (int y = 0; y < TS; y++)
                        {
                            int drawY = y + iTimes;
                            int drawYxWidth = drawY * width;

                            int yTimes = y * TS * bytesPerPixel;
                            int baseIndex = drawYxWidth * bytesPerPixel + jTimes * bytesPerPixel;

                            for (int x = 0; x < TS; x++)
                            {
                                int drawIndex = baseIndex + x * bytesPerPixel;
                                int srcIndex = yTimes + x * bytesPerPixel;

                                // Skip fully transparent pixels
                                if (pCurrentBytes[srcIndex + 3] == 0) continue;

                                // Copy the pixel data directly
                                pPixelData[drawIndex] = pCurrentBytes[srcIndex];
                                pPixelData[drawIndex + 1] = pCurrentBytes[srcIndex + 1];
                                pPixelData[drawIndex + 2] = pCurrentBytes[srcIndex + 2];
                                pPixelData[drawIndex + 3] = pCurrentBytes[srcIndex + 3];
                            }
                        }
                    }
                }
            }
        }

        // Get a pointer to the pixel buffer of the SKBitmap
        IntPtr pixelsPtr = drawer.skBitmap.GetPixels();

        // Ensure pixel data matches the bitmap dimensions and color type
        if (pixelsPtr != IntPtr.Zero)
        {
            // Copy the pixel data to the SKBitmap
            Marshal.Copy(pixelData, 0, pixelsPtr, pixelData.Length);
        }
    }

    private byte[] GetBytes(Color[,] pixels, int TS)
    {
        byte[] pixelData = new byte[TS * TS * 4];
        for (int y = 0; y < TS; y++)
        {
            for (int x = 0; x < TS; x++)
            {
                int index = (y * TS + x) * 4;
                Color color = pixels[y, x];
                byte[] argb = BitConverter.GetBytes(color.ToArgb());
                pixelData[index] = argb[0];
                pixelData[index + 1] = argb[1];
                pixelData[index + 2] = argb[2];
                pixelData[index + 3] = argb[3];
            }
        }
        return pixelData;
    }
    #endregion
}
