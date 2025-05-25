using SFML.Graphics;
using Shared;
using System.Security.Policy;
using System.Text.Json.Serialization;

namespace Royale2D
{
    public class MinimapSpriteData
    {
        public float overrideScaleX;
        public float overrideScaleY;
        public int topLeftX;
        public int topLeftY;
        public int bottomRightX;
        public int bottomRightY;

        [JsonIgnore]
        public Sprite sprite;

        public MinimapSpriteData()
        {
        }

        public void Init(string assetStringKey, string imageFullPath)
        {
            var texture = new Texture(imageFullPath);
            Assets.textures[assetStringKey] = texture;
            var minimapFrame = new Frame(new IntRect(0, 0, (int)texture.Size.X, (int)texture.Size.Y), 0, IntPoint.Zero, "");
            sprite = new Sprite([minimapFrame], assetStringKey);
            sprite.Init(assetStringKey);
            Assets.sprites[assetStringKey] = sprite;

            if (overrideScaleX == 0) overrideScaleX = 1;
            if (overrideScaleY == 0) overrideScaleY = 1;
            if (bottomRightX == 0) bottomRightX = (int)texture.Size.X;
            if (bottomRightY == 0) bottomRightY = (int)texture.Size.Y;
        }
    }

    public class Map
    {
        public string name;
        public MapWorkspace workspace;
        public MinimapSpriteData minimapSpriteData;
        public MinimapSpriteData minimapSmallSpriteData;

        // CLEANUP
        public bool initialized;
        public List<MapSection> sections = [];
        public MapSection mainSection;
        public Dictionary<int, TileData> tileDatas;
        public Dictionary<string, TileClump> tileClumps;
        public Dictionary<string, Texture> textures = new();
        public Dictionary<int, TileAnimation> tileAnimations = new();

        public Map(MapWorkspace workspace)
        {
            this.workspace = workspace;
            this.name = workspace.baseFolderPath.folderName;

            minimapSpriteData = workspace.minimapFilePath.DeserializeJson<MinimapSpriteData>();
            minimapSpriteData.Init(name + ":minimap", workspace.minimapImageFilePath.fullPath);
            minimapSpriteData.sprite.alignment = Alignment.TopLeft;

            minimapSmallSpriteData = workspace.minimapSmallFilePath.DeserializeJson<MinimapSpriteData>();
            minimapSmallSpriteData.Init(name + ":minimap_small", workspace.minimapSmallImageFilePath.fullPath);
            minimapSmallSpriteData.sprite.alignment = Alignment.BotRight;
            if (minimapSmallSpriteData.sprite!.texture!.Size!.X != 75 || minimapSmallSpriteData.sprite.texture.Size.Y != 75)
            {
                throw new Exception($"Minimap small image must be 75x75 pixels.");
            }
        }

        public void Init()
        {
            if (initialized) return;

            tileDatas = workspace.tilesetFilePath.DeserializeJson<Dictionary<int, TileData>>();

            List<TileAnimationModel> tileAnimationModels = workspace.tileAnimationFilePath.DeserializeJson<List<TileAnimationModel>>();

            foreach (TileData tile in tileDatas.Values)
            {
                if (tileAnimations.TryGetValue(tile.id, out TileAnimation? animation))
                {
                    tile.tileAnimation = animation;
                    continue;
                }

                TileAnimationModel? model = tileAnimationModels.FirstOrDefault(m => m.tileIds.Contains(tile.id));
                if (model == null) continue;

                tile.tileAnimation = new(tile.id, tileDatas, model);
                tileAnimations[tile.id] = tile.tileAnimation;
            }

            List<TileClump> tileClumpsList = workspace.tileClumpFilePath.DeserializeJson<List<TileClump>>();
            tileClumps = new Dictionary<string, TileClump>();
            foreach (TileClump tileClump in tileClumpsList)
            {
                tileClumps.Add(tileClump.name, tileClump);
                tileClump.Init();
            }

            var mapSectionFilePaths = workspace.mapSectionFolderPath.GetFiles(true, "json");

            // Populate map sections / virtual sections
            foreach (FilePath mapSectionFilePath in mapSectionFilePaths)
            {
                MapSection mapSection = JsonHelpers.DeserializeJsonFile<MapSection>(mapSectionFilePath);
                mapSection.Init(mapSectionFilePath.fullPath, this);

                sections.Add(mapSection);
                if (mapSection.name == "main")
                {
                    mainSection = mapSection;
                }
            }

            List<FilePath> tilesetImages = workspace.tilesetFolderPath.GetFiles(true, "png");
            foreach (FilePath path in tilesetImages)
            {
                string name = path.fileNameNoExt;
                Texture texture = new Texture(path.fullPath);
                textures.Add(name, texture);
            }

            foreach (TileData tile in tileDatas.Values)
            {
                tile.texture = textures[tile.imageFileName.Split('.')[0]];
            }

            // Populate indoor coordinate mappings to main for each non-main section
            // IMPROVE ideally, editor export would handle this
            Dictionary<string, GridZone> sectionNameToIndoorMapping = new Dictionary<string, GridZone>();
            foreach (MapSection mapSection in sections)
            {
                foreach (GridZone indoorMappingZone in mapSection.indoorMappingZones)
                {
                    if (mapSection.mainSectionChildPos != null)
                    {
                        indoorMappingZone.gridRect = indoorMappingZone.gridRect.Clone(mapSection.mainSectionChildPos.Value.i, mapSection.mainSectionChildPos.Value.j);
                    }
                    
                    sectionNameToIndoorMapping.Add(indoorMappingZone.name, indoorMappingZone);
                }

                // If the map section is a child of main section, automatically create an indoor mapping for it
                if (mapSection.mainSectionChildPos != null)
                {
                    var gridRect = new GridRect(
                        mapSection.mainSectionChildPos.Value.i, 
                        mapSection.mainSectionChildPos.Value.j, 
                        mapSection.mainSectionChildPos.Value.i + mapSection.gridHeight, 
                        mapSection.mainSectionChildPos.Value.j + mapSection.gridWidth
                    );
                    GridZone indoorMappingZone = new GridZone(mapSection.name, ZoneType.IndoorMapping, gridRect);
                    sectionNameToIndoorMapping.Add(mapSection.name, indoorMappingZone);
                }
            }
            foreach ((string sectionName, GridZone gridZone) in sectionNameToIndoorMapping)
            {
                MapSection section = sections.First(s => s.name == sectionName);
                section.indoorMappingToMain = gridZone.gridRect;
            }

            if (mainSection == null)
            {
                throw new Exception($"Map {name} has no map section called \"main\". There must be one that represents the main outdoor section you drop into from the \"battle bus\".");
            }

            if (mainSection.pixelWidth != mainSection.pixelWidth)
            {
                throw new Exception($"Map {name}'s main map section pixel width and height must be equal. This is an engine limitation and greatly simplifies minimap/storm logic.");
            }

            initialized = true;
        }

        public void RenderMinimap(Drawer drawer)
        {
            minimapSpriteData.sprite.Render(drawer, 0, 0, 0, overrideXScale: minimapSpriteData.overrideScaleX, overrideYScale: minimapSpriteData.overrideScaleY);
        }
    }
}
