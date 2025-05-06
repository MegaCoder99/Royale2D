using Shared;

namespace Royale2D
{
    // Used for efficient actor collision checking
    // PERF use some form of deterministic list that has O log N lookup time. Then remove the Contains() checks in this class.
    // (We can't use HashSet, iteration order is not netcode safe)
    public class ColliderGrid
    {
        public const int CellSize = 48;

        public Dictionary<GridCoords, List<ColliderComponent>> grid;
        public int width;
        public int height;

        public ColliderGrid(MapSection mapSection)
        {
            grid = new Dictionary<GridCoords, List<ColliderComponent>>();
            width = MyMath.DivideRoundUp(mapSection.pixelWidth, CellSize);
            height = MyMath.DivideRoundUp(mapSection.pixelHeight, CellSize);
        }

        public List<ColliderComponent> GetColliderComponents(IntRect myWorldRect, WorldSection section)
        {
            var ccsToReturn = new List<ColliderComponent>();
            List<GridCoords> gridCoords = Helpers.GetOverlappingGridCoords(width, height, myWorldRect, CellSize);
            foreach (GridCoords gridCoord in gridCoords)
            {
                // IMPROVE bounds check and assert since gridCoord should never give out of bounds values
                List<ColliderComponent>? currentCCs = grid.GetValueOrDefault(gridCoord);
                if (currentCCs != null)
                {
                    foreach (ColliderComponent cc in currentCCs)
                    {
                        if (ccsToReturn.Contains(cc)) continue;
                        ccsToReturn.Add(cc);
                    }
                }
            }

            return ccsToReturn;
        }

        public void UpdateInGrid(ColliderComponent colliderComponent, WorldSection section)
        {
            // Clear out current actor data in grid
            RemoveFromGrid(colliderComponent);
            
            IntRect boundingRect = colliderComponent.GetColliderBoundingRect();
            List<GridCoords> gridCoords = Helpers.GetOverlappingGridCoords(width, height, boundingRect, CellSize);
            foreach (GridCoords gridCoord in gridCoords)
            {
                List<ColliderComponent>? ccs = grid.GetValueOrDefault(gridCoord);
                if (ccs == null)
                {
                    ccs = new List<ColliderComponent>();
                    grid[gridCoord] = ccs;
                }

                if (!ccs.Contains(colliderComponent))
                {
                    ccs.Add(colliderComponent);
                }
                colliderComponent.gridCoords.Add(gridCoord);
            }
        }

        public void RemoveFromGrid(ColliderComponent colliderComponent)
        {
            foreach (GridCoords gridCoord in colliderComponent.gridCoords)
            {
                List<ColliderComponent>? ccs = grid.GetValueOrDefault(gridCoord);
                if (ccs != null)
                {
                    ccs.Remove(colliderComponent);
                    if (ccs.Count == 0)
                    {
                        grid.Remove(gridCoord);
                    }
                }
                
            }
            colliderComponent.gridCoords.Clear();
        }

        public void DrawDebug(Drawer drawer)
        {
            Rect screenRect = drawer.GetScreenRect();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float x = j * CellSize;
                    float y = i * CellSize;

                    if (x < screenRect.x1 - CellSize || x > screenRect.x2 || y < screenRect.y1 - CellSize || y > screenRect.y2)
                    {
                        continue;
                    }

                    drawer.DrawRectWH(x, y, CellSize, CellSize, true, Colors.DebugGreen, 1, Colors.DebugRed);
                    GridCoords gc = new GridCoords(i, j);
                    drawer.DrawText((grid.GetValueOrDefault(gc)?.Count() ?? 0).ToString(), x, y, fontType: FontType.Small);
                }
            }
        }
    }
}
