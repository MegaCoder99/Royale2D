using SFML.Graphics;
using SFML.System;
using Shared;

namespace Royale2D
{
    public class TextureManager
    {
        public Dictionary<string, RenderTexture> renderTextures = new Dictionary<string, RenderTexture>();
        public Dictionary<string, TileTextureManager> tileTextureManagers = new Dictionary<string, TileTextureManager>();

        public RenderTexture GetRenderTexture(string key)
        {
            if (!renderTextures.ContainsKey(key))
            {
                throw new Exception($"RenderTexture with key {key} does not exist.");
            }
            return renderTextures[key];
        }

        public RenderTexture AddRenderTexture(string key, uint width, uint height)
        {
            if (renderTextures.ContainsKey(key))
            {
                throw new Exception($"RenderTexture with key {key} already exists.");
            }
            renderTextures[key] = new RenderTexture(width, height);
            return renderTextures[key];
        }

        // CLEANUP
    }

    public class TileTextureManager
    {
        public RenderTexture[,] renderTextures;
        public Dictionary<GridCoords, int> lastTileAnimIndexDrawn;   // This lets us optimize tile animations by only re-drawing when they change

        public const int TextureSize = 1024;
        public MapSectionLayer mapSectionLayer;
        public TileData[,] tileGrid;
        public int layerIndex => mapSectionLayer.layerIndex;

        public TileTextureManager(MapSectionLayer mapSectionLayer)
        {
            this.mapSectionLayer = mapSectionLayer;
            lastTileAnimIndexDrawn = new Dictionary<GridCoords, int>();

            tileGrid = new TileData[mapSectionLayer.tileGrid.GetLength(0), mapSectionLayer.tileGrid.GetLength(1)];
            for (int i = 0; i < mapSectionLayer.tileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mapSectionLayer.tileGrid.GetLength(1); j++)
                {
                    int tileId = mapSectionLayer.tileGrid[i, j];
                    tileGrid[i, j] = mapSectionLayer.mapSection.map.tileDatas[tileId];
                }
            }

            renderTextures = new RenderTexture[
                MyMath.DivideRoundUp(tileGrid.GetLength(0) * 8, TextureSize),
                MyMath.DivideRoundUp(tileGrid.GetLength(1) * 8, TextureSize)
            ];

            LoadRenderTextures();
            //DrawInitialRenderTextures();
        }

        public void LoadRenderTextures()
        {
            for (int i = 0; i < renderTextures.GetLength(0); i++)
            {
                for (int j = 0; j < renderTextures.GetLength(1); j++)
                {
                    MapWorkspace mapWorkspace = mapSectionLayer.mapSection.map.workspace;
                    string layerImageFileName = mapWorkspace.GetMapSectionImageFileName(mapSectionLayer.mapSection.name, mapSectionLayer.layerIndex, i, j);
                    string layerImageFullPath = mapWorkspace.mapImageFolderPath.AppendFile(layerImageFileName).fullPath;

                    // Create the texture from file
                    Texture normalTexture = new Texture(layerImageFullPath);

                    // Initialize the RenderTexture
                    renderTextures[i, j] = new RenderTexture(TextureSize, TextureSize);

                    // Draw the loaded texture to the render texture
                    using (var sprite = new SFML.Graphics.Sprite(normalTexture))
                    {
                        renderTextures[i, j].Draw(sprite);
                        renderTextures[i, j].Display(); // finalize the drawing
                    }
                }
            }
        }

        public void DrawInitialRenderTextures()
        {
            for (int i = 0; i < tileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < tileGrid.GetLength(1); j++)
                {
                    TileData tileData = tileGrid[i, j];
                    if (tileData.tileAnimation != null) continue;

                    DrawToRenderTexture(i, j, tileData, renderTextures);
                }
            }
        }

        public void Render(Drawer drawer, WorldSectionLayer worldSectionLayer)
        {
            GridRect cameraGridRect = new GridRect(
                MyMath.ClampMin0(MyMath.Floor((drawer.pos.y - Game.HalfScreenH) / 8)),
                MyMath.ClampMin0(MyMath.Floor((drawer.pos.x - Game.HalfScreenW) / 8)),
                MyMath.ClampMax(MyMath.Floor((drawer.pos.y + Game.HalfScreenH) / 8), tileGrid.GetLength(0) - 1),
                MyMath.ClampMax(MyMath.Floor((drawer.pos.x + Game.HalfScreenW) / 8), tileGrid.GetLength(1) - 1)
            );
            //cameraGridRect = new GridRect(0, 0, maxI, maxJ);

            // This block is responsible for rendering animations, as well as tileInstance's Render method (which right now only used for internal hitbox debugging)
            for (int i = cameraGridRect.i1; i <= cameraGridRect.i2; i++)
            {
                for (int j = cameraGridRect.j1; j <= cameraGridRect.j2; j++)
                {
                    TileInstance? tileInstance = worldSectionLayer.GetTileInstance(i, j);
                    if (tileInstance == null) continue;

                    TileAnimation? tileAnimation = tileInstance.Value.tileAnimation;
                    if (tileAnimation != null)
                    {
                        int renderTextureI = MyMath.Floor((float)(i * 8) / TextureSize);
                        int renderTextureJ = MyMath.Floor((float)(j * 8) / TextureSize);

                        RenderTexture renderTexture = renderTextures[renderTextureI, renderTextureJ];

                        var sfmlSprite = new SFML.Graphics.Sprite(tileAnimation.texture);
                        sfmlSprite.Position = new Vector2f((j * 8) - (renderTextureJ * TextureSize), (i * 8) - (renderTextureI * TextureSize));
                        sfmlSprite.TextureRect = tileAnimation.GetTextureRect();

                        var gridCoords = new GridCoords(i, j);
                        if (!lastTileAnimIndexDrawn.ContainsKey(gridCoords) ||
                            lastTileAnimIndexDrawn[gridCoords] != tileAnimation.frameIndex)
                        {
                            renderTexture.Draw(sfmlSprite, new RenderStates(BlendMode.None));
                            lastTileAnimIndexDrawn[gridCoords] = tileAnimation.frameIndex;
                        }
                    }

                    tileInstance.Value.Render(drawer);
                }
            }

            // This block is responsible for rendering the render texture grid itself to render the tiles
            for (int i = 0; i < renderTextures.GetLength(0); i++)
            {
                for (int j = 0; j < renderTextures.GetLength(1); j++)
                {
                    Rect renderTextureRect = Rect.CreateWH(j * TextureSize, i * TextureSize, TextureSize, TextureSize);
                    if (drawer.GetScreenRect().Overlaps(renderTextureRect, true))
                    {
                        drawer.DrawTexture(renderTextures[i, j].Texture, j * TextureSize, i * TextureSize, ZIndex.FromLayerIndex(layerIndex, ZIndex.LayerOffsetTile));
                    }
                    if (Debug.showHitboxes)
                    {
                        drawer.DrawRectWH(j * TextureSize, i * TextureSize, TextureSize, TextureSize, false, SFML.Graphics.Color.Yellow, 1, zIndex: ZIndex.UIGlobal);
                    }
                }
            }
        }

        public void DrawToRenderTexture(int i, int j, TileData tileData, RenderTexture[,] renderTextureGrid)
        {
            int renderTextureI = MyMath.Floor((float)(i * 8) / TextureSize);
            int renderTextureJ = MyMath.Floor((float)(j * 8) / TextureSize);

            RenderTexture renderTexture = renderTextureGrid[renderTextureI, renderTextureJ];
            if (renderTexture == null)
            {
                renderTextureGrid[renderTextureI, renderTextureJ] = new RenderTexture(TextureSize, TextureSize);
                renderTexture = renderTextureGrid[renderTextureI, renderTextureJ];
                renderTexture.Clear(new SFML.Graphics.Color(0, 0, 0, 0));
                renderTexture.Display();
            }

            var sprite = new SFML.Graphics.Sprite(tileData.texture);
            sprite.Position = new Vector2f((j * 8) - (renderTextureJ * TextureSize), (i * 8) - (renderTextureI * TextureSize));
            sprite.TextureRect = new SFML.Graphics.IntRect(tileData.imageTopLeftPos.x, tileData.imageTopLeftPos.y, 8, 8);
            renderTexture.Draw(sprite);
        }
    }
}
