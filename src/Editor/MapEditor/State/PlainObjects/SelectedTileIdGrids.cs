using Shared;

namespace MapEditor;

// Abstracts multiple layer's worth of selected tile id grids, a tricky/complex use case
// Note: here and in other places, int? = null means absence of selection, and 0 is the Transparent Tile
public class SelectedTileIdGrids
{
    public Dictionary<int, int?[,]> layerToTileIdGrid;

    public bool singleLayer => layerToTileIdGrid.Count == 1;

    public int rows => layerToTileIdGrid.Values.FirstOrDefault()?.GetLength(0) ?? 0;
    public int cols => layerToTileIdGrid.Values.FirstOrDefault()?.GetLength(1) ?? 0;

    public SelectedTileIdGrids(int layer, int?[,] tileIdGrid)
    {
        layerToTileIdGrid = new();
        layerToTileIdGrid[layer] = tileIdGrid;
    }

    public SelectedTileIdGrids(Dictionary<int, int?[,]> layerToTileIdGrid)
    {
        this.layerToTileIdGrid = layerToTileIdGrid;
        // Remove all empty layers
        foreach (int layerIndex in layerToTileIdGrid.Keys.ToList())
        {
            if (layerToTileIdGrid[layerIndex].GetLength(0) == 0)
            {
                layerToTileIdGrid.Remove(layerIndex);
            }
        }
        // Validate all layers have the same grid dimensions
        if (layerToTileIdGrid.Values.Count > 1)
        {
            int?[,] firstGrid = layerToTileIdGrid.Values.First();
            foreach (int?[,] grid in layerToTileIdGrid.Values.Skip(1))
            {
                if (grid.GetLength(0) != firstGrid.GetLength(0) || grid.GetLength(1) != firstGrid.GetLength(1))
                {
                    throw new Exception("All layers must have the same grid dimensions");
                }
            }
        }
    }

    public bool IsEmpty()
    {
        return layerToTileIdGrid.Count == 0;
    }

    // If you have multiple layer's worth of selected tiles, this selects/"skims" the topmost VISIBLE tiles (meaning, a transparent tile on the top is ignored)
    public int?[,] GetTop()
    {
        var retVal = new int?[rows, cols];
        List<int> layerIndicesTopToBot = layerToTileIdGrid.Keys.ToList();
        layerIndicesTopToBot.Sort();
        layerIndicesTopToBot.Reverse();
        foreach (int layerIndex in layerIndicesTopToBot)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (retVal[i, j] == null || retVal[i, j] == Tile.TransparentTileId)
                    {
                        retVal[i, j] = layerToTileIdGrid[layerIndex][i, j] ?? retVal[i, j];
                    }
                }
            }
        }
        return retVal;
    }
}
