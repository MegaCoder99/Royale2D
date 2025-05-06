using Shared;

namespace Royale2D
{
    // PERF the grid should be tileinstances not tiledatas (check how much memory that uses though)
    public struct TileInstance
    {
        public int i;
        public int j;
        public WorldSectionLayer layer;
        
        public int layerIndex => layer.layerIndex;
        public TileData tileData;
        public Collider? collider => tileData.collider;
        public TileHitboxMode hitboxMode => tileData.hitboxMode;
        public bool IsLedge() => tileData.HasLedgeTag();
        public TileAnimation? tileAnimation => tileData.tileAnimation;

        public TileInstance(WorldSectionLayer layer, TileData tileData, int i, int j)
        {
            this.layer = layer;
            this.tileData = tileData;
            this.i = i;
            this.j = j;
        }

        public void Render(Drawer drawer)
        {
            if (Debug.showHitboxes && collider != null)
            {
                var color = Colors.DebugBlue;
                if (i == Debug.hitTileCoords?.i && j == Debug.hitTileCoords?.j) color = Colors.DebugRed;
                drawer.DrawPolygon(collider.GetTileInstanceWorldShape(this).GetPoints(), color, true, ZIndex.UIGlobal);
            }
        }

        /*
        // Need for this is eliminated with tags, a more generic system
        public TileClumpInstance? GetTileClumpInstanceFromName(params string[] tileClumpNames)
        {
            string key = new GridCoords(i, j).ToString();
            if (layer.tileClumpInstanceCache.ContainsKey(key))
            {
                TileClumpInstance tileClumpInstance = layer.tileClumpInstanceCache[key];
                if (tileClumpNames.Contains(tileClumpInstance.tileClump.name))
                {
                    return tileClumpInstance;
                }
            }
            return null;
        }
        */

        public TileClumpInstance? GetTileClumpInstanceFromTag(params string[] tags)
        {
            string key = new GridCoords(i, j).ToString();
            if (layer.tileClumpInstanceCache.ContainsKey(key))
            {
                TileClumpInstance tileClumpInstance = layer.tileClumpInstanceCache[key];
                if (tags.Any(t => tileClumpInstance.tileClump.tags.Contains(t)))
                {
                    return tileClumpInstance;
                }
            }
            return null;
        }

        public bool HasTileClumpTag(params string[] tags)
        {
            return GetTileClumpInstanceFromTag(tags) != null;
        }

        /*
        public Point GetCenterPos()
        {
            return new Point((j * 8) + 4, (i * 8) + 4);
        }
        */

        public IntPoint GetWorldCenterPos()
        {
            return new IntPoint((j * 8) + 4, (i * 8) + 4);
        }

        public IntPoint GetLedgeDir()
        {
            IntPoint dir;
            
            if (tileData.HasTag(TileTag.LedgeLeft)) dir = new IntPoint(-1, 0);
            else if (tileData.HasTag(TileTag.LedgeRight)) dir = new IntPoint(1, 0);
            else if (tileData.HasTag(TileTag.LedgeUp)) dir = new IntPoint(0, -1);
            else if (tileData.HasTag(TileTag.LedgeDown)) dir = new IntPoint(0, 1);
            else if (tileData.HasTag(TileTag.LedgeUpLeft)) dir = new IntPoint(-1, -1);
            else if (tileData.HasTag(TileTag.LedgeUpRight)) dir = new IntPoint(1, -1);
            else if (tileData.HasTag(TileTag.LedgeDownLeft)) dir = new IntPoint(-1, 1);
            else if (tileData.HasTag(TileTag.LedgeDownRight)) dir = new IntPoint(1, 1);
            else
            {
                return IntPoint.Zero;
            }

            return dir;
        }

        public bool CanNudgeLeft(int yDir)
        {
            TileInstance? td1 = layer.GetTileInstance(i, j - 1);
            TileInstance? td2 = layer.GetTileInstance(i, j - 2);
            if (yDir == 1 && td1?.hitboxMode == TileHitboxMode.DiagTopRight || yDir == -1 && td1?.hitboxMode == TileHitboxMode.DiagBotRight) return false;
            return td1?.hitboxMode != TileHitboxMode.FullTile && td2?.hitboxMode == TileHitboxMode.None;
        }

        public bool CanNudgeRight(int yDir)
        {
            TileInstance? td1 = layer.GetTileInstance(i, j + 1);
            TileInstance? td2 = layer.GetTileInstance(i, j + 2);
            if (yDir == 1 && td1?.hitboxMode == TileHitboxMode.DiagTopLeft || yDir == -1 && td1?.hitboxMode == TileHitboxMode.DiagBotLeft) return false;
            return td1?.hitboxMode != TileHitboxMode.FullTile && td2?.hitboxMode == TileHitboxMode.None;
        }

        public bool CanNudgeUp(int xDir)
        {
            TileInstance? td1 = layer.GetTileInstance(i - 1, j);
            TileInstance? td2 = layer.GetTileInstance(i - 2, j);
            if (xDir == 1 && td1?.hitboxMode == TileHitboxMode.DiagBotLeft || xDir == -1 && td1?.hitboxMode == TileHitboxMode.DiagBotRight) return false;
            return td1?.hitboxMode != TileHitboxMode.FullTile && td2?.hitboxMode == TileHitboxMode.None;
        }

        public bool CanNudgeDown(int xDir)
        {
            TileInstance? td1 = layer.GetTileInstance(i + 1, j);
            TileInstance? td2 = layer.GetTileInstance(i + 2, j);
            if (xDir == 1 && td1?.hitboxMode == TileHitboxMode.DiagTopLeft || xDir == -1 && td1?.hitboxMode == TileHitboxMode.DiagTopRight) return false;
            return td1?.hitboxMode != TileHitboxMode.FullTile && td2?.hitboxMode == TileHitboxMode.None;
        }

        // Returns the number of tiles with FullTile collision mode in the xDir and yDir specified assuming this TileInstance is a ledge tile
        public int GetTileLedgeHeight(int xDir, int yDir)
        {
            int height = 0;
            if (xDir == 0 && yDir == 0)
            {
                throw new Exception("xDir and yDir cannot both be 0");
                // return Point.zero;
            }

            while (true)
            {
                bool solidTileFound = false;
                // Need to check the layer below too for multi-layered map sections
                for (int l = layerIndex; l >= layerIndex - 1 && l >= 0; l--)
                {
                    TileInstance? tileData = layer.section.layers[l].GetTileInstance(i + yDir, j + xDir);
                    if (tileData?.hitboxMode == TileHitboxMode.FullTile)
                    {
                        xDir += Math.Sign(xDir);
                        yDir += Math.Sign(yDir);
                        solidTileFound = true;
                        height++;
                    }
                }

                if (!solidTileFound) break;
            }

            return height;
        }

        public bool CanLedgeJump()
        {
            IntPoint ledgeDir = GetLedgeDir();
            if (ledgeDir.IsZero()) return false;
            int tileHeight = GetTileLedgeHeight(ledgeDir.x, ledgeDir.y);
            if (tileHeight <= 0) return false;
            return true;
        }

        public bool Equals(TileInstance other)
        {
            return i == other.i && j == other.j && layer == other.layer;
        }
    }
}
