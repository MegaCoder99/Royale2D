using Shared;

namespace Royale2D
{
    public class WorldSectionLayer
    {
        public int layerIndex;
        public WorldSection section;
        public Dictionary<int, TileData> tileDatas => section.mapSection.map.tileDatas;
        public Dictionary<string, TileClump> tileClumps => section.mapSection.map.tileClumps;

        public Dictionary<string, TileClumpInstance> tileClumpInstanceCache;

        public string layerKey => section.mapSection.name + ":" + layerIndex;

        public MapSectionLayer mapSectionLayer;

        public TileData[,] tileGrid;

        public TextureManager textureManager => section.world.textureManager;
        public TileTextureManager tileTextureManager;

        public WorldSectionLayer(int layerIndex, WorldSection section)
        {
            this.layerIndex = layerIndex;
            this.section = section;

            mapSectionLayer = section.mapSection.layers[layerIndex];

            tileClumpInstanceCache = mapSectionLayer.CreateTileClumpInstanceCache(tileClumps.Values.ToList());

            tileGrid = new TileData[mapSectionLayer.tileGrid.GetLength(0), mapSectionLayer.tileGrid.GetLength(1)];
            for (int i = 0; i < mapSectionLayer.tileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mapSectionLayer.tileGrid.GetLength(1); j++)
                {
                    int tileId = mapSectionLayer.tileGrid[i, j];
                    tileGrid[i, j] = mapSectionLayer.mapSection.map.tileDatas[tileId];
                }
            }

            tileTextureManager = (textureManager.tileTextureManagers[layerKey] = new TileTextureManager(mapSectionLayer));
        }

        public void Update()
        {
        }

        public void Render(Drawer drawer)
        {
            tileTextureManager.Render(drawer, this);
        }

        public TileInstance? GetTileInstance(int i, int j)
        {
            if (!tileGrid.InRange(i, j)) return null;

            return new TileInstance(this, tileGrid[i, j], i, j);
        }

        public void TransformTileClumpWithAnim(TileClumpInstance tileClumpInstance, Actor creator, string? overrideTransformClumpName = null)
        {
            new Anim(creator, tileClumpInstance.GetCenterPos(), tileClumpInstance.tileClump.breakSprite, new AnimOptions { soundName = tileClumpInstance.tileClump.breakSound });
            TransformTileClump(tileClumpInstance, overrideTransformClumpName);
        }

        public void TransformTileClump(TileClumpInstance tileClumpInstance, string? overrideTransformClumpName = null)
        {
            if (!tileClumps.ContainsKey(overrideTransformClumpName ?? tileClumpInstance.tileClump.transformClumpName)) return;

            WorldSectionLayer? layerAbove = section.layers.SafeGet(layerIndex + 1);

            TileClump newTileClump = tileClumps[overrideTransformClumpName ?? tileClumpInstance.tileClump.transformClumpName];
            for (int i = tileClumpInstance.i1; i <= tileClumpInstance.i2; i++)
            {
                for (int j = tileClumpInstance.j1; j <= tileClumpInstance.j2; j++)
                {
                    bool isSubsection = tileClumpInstance.tileClump.subsections.Any(ss => ss.cells.Any(c => c.i == i - tileClumpInstance.i1 && c.j == j - tileClumpInstance.j1));
                    int newTileId = newTileClump.tileIds[i - tileClumpInstance.i1, j - tileClumpInstance.j1];
                    if (isSubsection && layerAbove != null)
                    {
                        layerAbove.TransformTile(i, j, newTileId);
                    }

                    TransformTile(i, j, newTileId);
                }
            }
        }

        public void TransformTile(int i, int j, int newTileId)
        {
            tileGrid[i, j] = tileDatas[newTileId];
            tileTextureManager.DrawToRenderTexture(i, j, tileDatas[newTileId], tileTextureManager.renderTextures);

            string key = new GridCoords(i, j).ToString();
            tileClumpInstanceCache.Remove(key);
        }
    }
}
