namespace MapEditor;

public partial class State
{
    /*
    [Script("st", "Get similar tiles")]
    public void GetSimilarTiles(string[] args)
    {
        if (lastSelectedSectionsSC.selectedTileCoords.Count != 1)
        {
            Prompt.ShowError("Please select a single tile to find similar tiles");
            return;
        }

        MapSectionLayer? selectedLayer = lastSelectedSectionsSC.GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        GridCoords selectedTileCoord = lastSelectedSectionsSC.selectedTileCoords[0];
        Tile selectedTile = tileset.GetTileById(selectedLayer.tileGrid[selectedTileCoord.i, selectedTileCoord.j]);
        if (selectedTile.id == 0)
        {
            Prompt.ShowError("Please select a non-transparent tile");
            return;
        }

        List<Tile> similarTiles = [selectedTile];
        foreach (Tile tile in tileset.GetAllTiles().OrderBy(tile => tile.id))
        {
            if (tile.id == Tile.TransparentTileId || tile.id == selectedTile.id) continue;

            if (IsTileSimilar(tile, selectedTile))
            {
                similarTiles.Add(tile);
            }
        }

        int TS = tileset.tileSize;
        Bitmap tileClumpBitmap = new Bitmap(TS * similarTiles.Count, TS);
        var lockBitmap = new LockedBitmap(tileClumpBitmap);
        lockBitmap.LockBits();

        for (int i = 0; i < similarTiles.Count; i++)
        {
            var colors = tileset.GetColorsFromTileHash(similarTiles[i].hash);
            for (int y = 0; y < TS; y++)
            {
                for (int x = 0; x < TS; x++)
                {
                    lockBitmap.SetPixel(x + (i * TS), y, colors[y, x]);
                }
            }
        }

        lockBitmap.UnlockBits();
        tileClumpBitmap.Save(FolderPath.GetDesktopRawFolderPath() + "/similar_tiles.png");

        Prompt.ShowMessage("Found " + similarTiles.Count + " similar tiles. Saved to desktop as similar_tiles.png");
    }

    [Script("mt", "Merge similar tiles")]
    public void MergeSimilarTiles(string[] args)
    {
        int similarTileIndex = int.Parse(args[0]);

        if (lastSelectedSectionsSC.selectedTileCoords.Count != 1)
        {
            Prompt.ShowError("Please select a single tile to find similar tiles");
            return;
        }

        MapSectionLayer? selectedLayer = lastSelectedSectionsSC.GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        GridCoords selectedTileCoord = lastSelectedSectionsSC.selectedTileCoords[0];
        Tile selectedTile = tileset.GetTileById(selectedLayer.tileGrid[selectedTileCoord.i, selectedTileCoord.j]);
        if (selectedTile.id == 0)
        {
            Prompt.ShowError("Please select a non-transparent tile");
            return;
        }

        List<Tile> similarTiles = [selectedTile];
        foreach (Tile tile in tileset.GetAllTiles().OrderBy(tile => tile.id))
        {
            if (tile.id == Tile.TransparentTileId || tile.id == selectedTile.id) continue;

            if (IsTileSimilar(tile, selectedTile))
            {
                similarTiles.Add(tile);
            }
        }
        HashSet<int> similarTilesIds = new(similarTiles.Select(tile => tile.id));

        Tile tileToUse = similarTiles[similarTileIndex];

        foreach (MapSection mapSection in GetAllSections())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (similarTilesIds.Contains(tileId) && tileId != tileToUse.id)
                        {
                            layer.ChangeTileGrid([new TileInstance(i, j, tileToUse.id)]);
                        }
                    }
                }
            }
        }

        RedrawAll();
        ForceSaveAll();

        Prompt.ShowMessage("Merged similar tiles");
    }
    */
}
