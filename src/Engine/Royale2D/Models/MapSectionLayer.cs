using Shared;
using System.Text.Json.Serialization;

namespace Royale2D
{
    public record MapSectionLayer
    {
        [JsonConverter(typeof(TileGridConverter))]
        public int[,] tileGrid;

        public MapSection mapSection;
        public int layerIndex;

        public void Init(MapSection mapSection, int layerIndex)
        {
            this.mapSection = mapSection;
            this.layerIndex = layerIndex;
        }

        // Engine code can use this to generate a tile clump cache on map load. Key is GridCoords ToString() representation.
        // If this ends up being too slow to load, can do it on export, though this does increase export complexity and file size
        public Dictionary<string, TileClumpInstance> CreateTileClumpInstanceCache(List<TileClump> tileClumps)
        {
            var cache = new Dictionary<string, TileClumpInstance>();
            for (int i = 0; i < tileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < tileGrid.GetLength(1); j++)
                {
                    foreach (TileClump clump in tileClumps)
                    {
                        if (clump.CheckIfClumpMatches(tileGrid, i, j))
                        {
                            for (int k = i; k < i + clump.rows; k++)
                            {
                                for (int l = j; l < j + clump.cols; l++)
                                {
                                    cache[GridCoords.ToString(k, l)] = new TileClumpInstance(clump, i, j);
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return cache;
        }
    }
}
