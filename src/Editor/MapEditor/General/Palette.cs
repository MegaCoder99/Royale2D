using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public class PaletteData
{
    public Dictionary<Tile, Dictionary<Color, int>> tileColorToCount;
    public Dictionary<Tile, int> tileToCount;
    public Dictionary<Color, int> globalColorToCount;
    public Dictionary<Color, List<Color>> colorGroups;
    public Dictionary<Color, Color> colorToMasterColor;

    public PaletteData(State state)
    {
        Tileset tileset = state.tileset;

        tileColorToCount = [];
        foreach (Tile tile in tileset.GetAllTiles())
        {
            if (tile.id == 0) continue;
            Dictionary<Color, int> colorToCount = [];
            Color[,] colors = tileset.GetColorsFromTileHash(tile.hash, false);
            for (int y = 0; y < tileset.tileSize; y++)
            {
                for (int x = 0; x < tileset.tileSize; x++)
                {
                    Color color = colors[y, x];
                    if (!colorToCount.ContainsKey(color))
                    {
                        colorToCount[color] = 0;
                    }
                    colorToCount[color]++;
                }
            }
            tileColorToCount[tile] = colorToCount;
        }

        tileToCount = [];
        foreach ((MapSection mapSection, SectionsSC sectionsSC) in state.GetAllSectionsWithSectionsSC())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (tileId == 0) continue;
                        Tile tile = tileset.GetTileById(tileId);
                        if (!tileColorToCount.ContainsKey(tile)) continue;
                        if (!tileToCount.ContainsKey(tile))
                        {
                            tileToCount[tile] = 0;
                        }
                        tileToCount[tile] += tileColorToCount[tile].Count;
                    }
                }
            }
        }

        globalColorToCount = [];
        foreach ((Tile tile, int tileCount) in tileToCount)
        {
            // if (!tileColorToCount.ContainsKey(tile)) continue;
            foreach ((Color color, int colorCount) in tileColorToCount[tile])
            {
                if (!globalColorToCount.ContainsKey(color))
                {
                    globalColorToCount[color] = 0;
                }
                globalColorToCount[color] += colorCount * tileCount;
            }
        }

        // Order colors by count
        List<KeyValuePair<Color, int>> orderedColorKvps = globalColorToCount.ToList();
        orderedColorKvps.Sort((a, b) => b.Value.CompareTo(a.Value));

        // Group colors by similarity
        List<Color> orderedColors = orderedColorKvps.Select(kvp => kvp.Key).ToList();
        colorGroups = [];
        foreach (Color color in orderedColors)
        {
            bool foundGroup = false;
            foreach (Color groupColor in colorGroups.Keys)
            {
                if (Helpers.GetColorDifference(color, groupColor) < 16)
                {
                    colorGroups[groupColor].Add(color);
                    foundGroup = true;
                    break;
                }
            }
            if (!foundGroup)
            {
                colorGroups[color] = [color];
            }
        }

        colorToMasterColor = [];
        foreach ((Color masterColor, List<Color> groupColors) in colorGroups)
        {
            colorToMasterColor[masterColor] = masterColor;
            foreach (Color color in groupColors)
            {
                colorToMasterColor[color] = masterColor;
            }
        }
    }

    public Dictionary<Color, Color> closestColorCache = [];
    public Color GetClosestColor(Color color)
    {
        if (closestColorCache.ContainsKey(color)) return closestColorCache[color];

        var retVal = globalColorToCount.Keys
            .Where(paletteColor => Helpers.GetColorDifference(color, paletteColor) < 16)
            .OrderByDescending(paletteColor => globalColorToCount[paletteColor])
            .FirstOrDefault(color); // Default to input color if no match is found

        closestColorCache[color] = retVal;
        return closestColorCache[color];
    }

    public string GetCloseTileHash(Tileset tileset, string tileHash)
    {
        Color[,] newColorGrid = tileset.GetColorsFromTileHash(tileHash, true);
        for (int y = 0; y < tileset.tileSize; y++)
        {
            for (int x = 0; x < tileset.tileSize; x++)
            {
                newColorGrid[y, x] = GetClosestColor(newColorGrid[y, x]);
            }
        }
        return tileset.GetTileHashFromColors(newColorGrid);
    }
}
