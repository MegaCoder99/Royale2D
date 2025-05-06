namespace Royale2D
{
    public abstract class BigMinimapScreen
    {
        public World world;
        public int gridWidth;
        public int gridHeight;
        public Minimap minimap;

        public List<Character> characters => world.characters;

        public BigMinimapScreen(World world, Storm storm, MinimapSpriteData minimapSpriteData, string renderTextureName)
        {
            this.world = world;

            gridWidth = world.map.mainSection.gridWidth;
            gridHeight = world.map.mainSection.gridHeight;

            minimap = new Minimap(
                mapWidth: world.map.mainSection.pixelWidth,
                mapHeight: world.map.mainSection.pixelHeight,
                minimapWidth: Game.ScreenW,
                minimapHeight: Game.ScreenH,
                startX: (int)(minimapSpriteData.topLeftX * minimapSpriteData.overrideScaleX),
                startY: (int)(minimapSpriteData.topLeftY * minimapSpriteData.overrideScaleY),
                endX: (int)(minimapSpriteData.bottomRightX * minimapSpriteData.overrideScaleX),
                endY: (int)(minimapSpriteData.bottomRightY * minimapSpriteData.overrideScaleY),
                storm,
                renderTextureName);
        }

        // Use BFS to find the closest droppable tile
        public void MoveToNearestDroppableTile(BattleBusData battleBusData)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            int[] di = { 0, 0, 1, -1 };
            int[] dj = { 1, -1, 0, 0 };
            int startI = battleBusData.dropI;
            int startJ = battleBusData.dropJ;
            queue.Enqueue((startI, startJ));
            visited.Add((startI, startJ));
            while (queue.Count > 0)
            {
                (int i, int j) = queue.Dequeue();
                if (CanLand(i, j))
                {
                    battleBusData.dropIFloat = i;
                    battleBusData.dropJFloat = j;
                    return;
                }
                for (int d = 0; d < 4; d++)
                {
                    int ni = i + di[d];
                    int nj = j + dj[d];
                    if (InBounds(ni, nj) && !visited.Contains((ni, nj)))
                    {
                        visited.Add((ni, nj));
                        queue.Enqueue((ni, nj));
                    }
                }
            }
            throw new InvalidOperationException("No droppable tile found. This should never happen unless map is misconfigured.");
        }

        public WorldSection GetSectionFromGridCoords(int i, int j)
        {
            foreach (WorldSection section in world.sections)
            {
                if (section.mapSection.mainSectionChildPos != null)
                {
                    GridCoords sectionGridCoordsTopLeft = section.mapSection.mainSectionChildPos.Value;

                    int newI = i - sectionGridCoordsTopLeft.i;
                    int newJ = j - sectionGridCoordsTopLeft.j;

                    if (newI >= 0 && newI < section.mapSection.gridHeight && newJ >= 0 && newJ < section.mapSection.gridWidth)
                    {
                        // Prevents dropping in mountain region below turtle rock (it's concave so need this code to handle that)
                        if (section.layers.Any(layer => layer.tileGrid[newI, newJ].id != 0))
                        {
                            return section;
                        }
                    }
                }
            }

            return world.mainSection;
        }

        public bool InBounds(int i, int j)
        {
            if (i < 0 || i > gridHeight - 1 || j < 0 || j > gridWidth - 1) return false;
            return true;
        }

        public bool CanLand(int i, int j)
        {
            int newI = i;
            int newJ = j;
            WorldSection section = GetSectionFromGridCoords(i, j);
            if (section.mapSection.mainSectionChildPos != null)
            {
                newI = i - section.mapSection.mainSectionChildPos.Value.i;
                newJ = j - section.mapSection.mainSectionChildPos.Value.j;
            }

            // There must be a 2x2 area of tiles that all can land
            if (i > 0 && j > 0 &&
                section.firstTileGrid[newI - 1, newJ - 1].CanLand() &&
                section.firstTileGrid[newI - 1, newJ].CanLand() &&
                section.firstTileGrid[newI, newJ - 1].CanLand() &&
                section.firstTileGrid[newI, newJ].CanLand())
            {
                return true;
            }
            if (i < gridHeight - 1 && j < gridWidth - 1 &&
                section.firstTileGrid[newI + 1, newJ + 1].CanLand() &&
                section.firstTileGrid[newI + 1, newJ].CanLand() &&
                section.firstTileGrid[newI, newJ + 1].CanLand() &&
                section.firstTileGrid[newI, newJ].CanLand())
            {
                return true;
            }

            return false;
        }
    }
}
