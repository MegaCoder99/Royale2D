using Editor;
using Shared;
using System.Drawing;

namespace MapEditor;

public partial class State
{
    public void AddScriptHotkeys(HotkeyManager hotkeyManager)
    {
        hotkeyManager.AddHotkeys([
            //new HotkeyConfig(Key.F1, MoveSelectedVirtualSection),
        ], null);
    }

    // Given a tile hash prefix (the first arg of the script), returns the colors represented by it
    [Script("gt", "Get Tile Hash Prefix Colors")]
    public void GetTileHashPrefixColors(string[] args)
    {
        string tileHashPrefix = string.Join(' ', args);
        List<Color> colors = mapSectionsSC.tileset.GetColorPoolFromTileHashPrefix(tileHashPrefix);
        Prompt.ShowMessage("Colors: " + string.Join("\n", colors));
    }

    // Already runs before every save, but you can run it in a script to test it manually at any time
    [Script("v", "Validate")]
    public void ValidateBeforeSaveScript(string[] args)
    {
        if (ValidateBeforeSave())
        {
            Prompt.ShowMessage("No validation errors");
        }
    }

    // For if you need to save everything (to test the save code, or if dirty and enabling save button isn't working due to some bug)
    [Script("fs", "Force Save All")]
    public void ForceSaveAllScript(string[] args)
    {
        ForceSaveAll();
        Prompt.ShowMessage("Done");
    }

    // Exports the selected tiles to a PNG image file of your choosing.
    [Script("ef", "Export tiles to image file")]
    public void ExportTilesAsImageFile(string[] args)
    {
        lastSelectedSectionsSC.TilePixelsExportHelper(false, false);
    }

    // Imports tiles from a PNG image into the selected cell coordinate as the top left position.
    [Script("if", "Import tiles from image file")]
    public void ImportTilesAsImageFile(string[] args)
    {
        string filePath = Prompt.SelectFile("Select image file to import", "", "png");
        if (!filePath.IsSet()) return;
        Bitmap bitmap = BitmapHelpers.CreateBitmapFromFile(FilePath.New(filePath));
        lastSelectedSectionsSC.TilePixelsImportHelper(bitmap);
    }

    [Script("sr")]
    public void ShowRarelyUsedTiles(string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out int count))
        {
            Prompt.ShowError("Provide 1 arg, the max count");
            return;
        }

        if (showRarelyUsedTilesCount > 0 && count == showRarelyUsedTilesCount)
        {
            HideRarelyUsedTiles([]);
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, [], () =>
        {
            tileToCountForSrScript = GetTileToCount();
            showRarelyUsedTilesCount = count;
        });
    }

    [Script("hr")]
    public void HideRarelyUsedTiles(string[] args)
    {
        context.ApplyCodeCommit(RedrawData.ToolingAll, [], () =>
        {
            showRarelyUsedTilesCount = 0;
        });
    }

    [Script("ct")]
    public void CorrectTileHashes(string[] args)
    {
        foreach (var tile in tileset.GetAllTiles())
        {
            var colors = tileset.GetColorsFromTileHash(tile.hash, true);
            string newHash = tileset.GetTileHashFromColors(colors);
            if (tile.hash != newHash)
            {
                tile.hash = newHash;
                Console.WriteLine($"Correcting tile hash {tile.id} from {tile.hash} to {newHash}");
            }
        }
        ForceSaveAll();
        Prompt.ShowMessage("Done");
    }

    [Script("cc")]
    public void ColorCorrectSelection(string[] args)
    {
        int tileCountToCorrect = int.Parse(args[0]);
        int threshold = args.Length > 1 ? int.Parse(args[1]) : 32;

        if (lastSelectedSectionsSC.selectedTileCoords.Count < 2)
        {
            Prompt.ShowError("Please select at least two tiles");
            return;
        }

        if (lastSelectedSectionsSC.selectedTileCoords.Count <= tileCountToCorrect)
        {
            Prompt.ShowError("Must select more tiles than tileCountToCorrect");
            return;
        }

        MapSectionLayer? selectedLayer = lastSelectedSectionsSC.GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        List<Tile> selectedTiles = [];
        for (int i = 0; i < tileCountToCorrect; i++)
        {
            GridCoords selectedTileCoord = lastSelectedSectionsSC.selectedTileCoords[i];
            Tile selectedTile = tileset.GetTileById(selectedLayer.tileGrid[selectedTileCoord.i, selectedTileCoord.j]);
            if (selectedTile.id == 0) continue;
            selectedTiles.Add(selectedTile);
        }

        List<Tile> referenceTiles = [];
        for (int i = tileCountToCorrect; i < lastSelectedSectionsSC.selectedTileCoords.Count; i++)
        {
            GridCoords selectedTileCoord = lastSelectedSectionsSC.selectedTileCoords[i];
            Tile selectedTile = tileset.GetTileById(selectedLayer.tileGrid[selectedTileCoord.i, selectedTileCoord.j]);
            if (selectedTile.id == 0) continue;
            referenceTiles.Add(selectedTile);
        }

        ColorCorrectInternal(selectedTiles, referenceTiles, threshold);
    }

    public void ColorCorrectInternal(List<Tile> selectedTiles, List<Tile> referenceTiles, int threshold)
    {
        HashSet<Color> selectedReferenceTileColors = [];
        foreach (Tile referenceTile in referenceTiles)
        {
            List<Color> colors = tileset.GetColorsFromTileHash(referenceTile.hash, false).ToList();
            foreach (Color color in colors)
            {
                selectedReferenceTileColors.Add(color);
            }
        }

        int subsitutionCount = 0;
        Dictionary<Color, Color> colorSubstitutions = [];
        HashSet<Tile> tilesUsedForReplacement = [];
        int newTileCount = 0;
        Dictionary<int, int> tileIdsToReplace = [];
        foreach (Tile selectedTile in selectedTiles)
        {
            if (tilesUsedForReplacement.Contains(selectedTile)) continue;  // This indicates the tile has already been corrected since it turned into a new one we created

            Color[,] newColors = tileset.GetColorsFromTileHash(selectedTile.hash, true);
            for (int y = 0; y < newColors.GetLength(0); y++)
            {
                for (int x = 0; x < newColors.GetLength(1); x++)
                {
                    Color color = newColors[y, x];

                    if (color.A == 0) continue;
                    if (colorSubstitutions.ContainsKey(color))
                    {
                        newColors[y, x] = colorSubstitutions[color];
                        continue;
                    }

                    int bestDifference = int.MaxValue;
                    Color? bestColor = null;
                    foreach (Color referenceColor in selectedReferenceTileColors)
                    {
                        int difference = Helpers.GetColorDifference(color, referenceColor);
                        if (difference < bestDifference)
                        {
                            bestDifference = difference;
                            bestColor = referenceColor;
                        }
                    }
                    if (bestDifference < threshold)
                    {
                        newColors[y, x] = bestColor!.Value;
                        colorSubstitutions[color] = bestColor!.Value;
                    }
                    else
                    {
                        colorSubstitutions[color] = color;
                    }
                }
            }

            string newColorTileHash = tileset.GetTileHashFromColors(newColors);

            Tile? newTileWithCorrectedColors = tileset.GetFirstTileByHash(newColorTileHash);

            if (newTileWithCorrectedColors == null)
            {
                newTileCount++;
                newTileWithCorrectedColors = tileset.AddNewTile(newColorTileHash);
            }
            else
            {
                if (newTileWithCorrectedColors.hash == selectedTile.hash) continue;
            }

            tilesUsedForReplacement.Add(newTileWithCorrectedColors);

            int oldTileId = selectedTile.id;
            int newTileId = newTileWithCorrectedColors.id;

            if (newTileId == Tile.TransparentTileId || oldTileId == newTileId) return;
            if (oldTileId == Tile.TransparentTileId) return;

            tileIdsToReplace[oldTileId] = newTileId;

            foreach ((MapSection mapSection, SectionsSC sectionsSC) in GetAllSectionsWithSectionsSC())
            {
                foreach (MapSectionLayer layer in mapSection.layers)
                {
                    for (int k = 0; k < layer.tileGrid.GetLength(0); k++)
                    {
                        for (int l = 0; l < layer.tileGrid.GetLength(1); l++)
                        {
                            int tileId = layer.tileGrid[k, l];
                            if (tileId == oldTileId)
                            {
                                if (layer.ChangeTileGrid([new TileInstance(k, l, newTileId)]))
                                {
                                    subsitutionCount++;
                                }
                            }
                        }
                    }
                }
            }
        }

        tileset.ReplaceTileForeignKeyRefs(tileIdsToReplace, this);

        // IMPROVE avoid forcesaveall. properly set undo queue. compare with PasteAndMergeTiles(). see if code can be reused. scripts in general need to be re-evaluated.
        ForceSaveAll();

        Prompt.ShowMessage($"Added {newTileCount} new tiles. Replaced {subsitutionCount} cells. Saved to disk.");
    }

    // Moves the selected virtual section to the selected tile. All tiles and instances inside of it are moved.
    // Before this can be productized, we must:
    // -Make sure it doesn't overlap with another virtual section
    // -Does not go out-of-bounds
    // -Currently a bug where it doesn't move entrances close to the edge
    // -Test it carefully (corner cases, undo, etc)
    [Script("mv", "Move selected virtual section")]
    public void MoveSelectedVirtualSection(string[] args)
    {
        if (lastSelectedSectionsSC.selectedTileCoords.Count != 1)
        {
            Prompt.ShowError("Please select a single tile to move to");
            return;
        }
        if (lastSelectedSectionsSC.selectedZone?.zoneType != ZoneTypes.VirtualSection.name)
        {
            Prompt.ShowError("Please select a virtual section to move");
            return;
        }

        int TS = tileset.TS;
        GridRectSC gridRect = lastSelectedSectionsSC.selectedZone.gridRect!;
        int offsetI = lastSelectedSectionsSC.selectedTileCoords[0].i - gridRect.i1;
        int offsetJ = lastSelectedSectionsSC.selectedTileCoords[0].j - gridRect.j1;
        Dictionary<MapSectionLayer, List<TileInstance>> tileInstances = new();
        foreach (MapSectionLayer layer in lastSelectedSectionsSC.selectedMapSection.layers)
        {
            for (int i = gridRect.i1; i <= gridRect.i2; i++)
            {
                for (int j = gridRect.j1; j <= gridRect.j2; j++)
                {
                    if (!tileInstances.ContainsKey(layer))
                    {
                        tileInstances[layer] = new List<TileInstance>();
                    }

                    int tileId = layer.tileGrid[i, j];
                    tileInstances[layer].Add(new TileInstance(i, j, Tile.TransparentTileId));
                    tileInstances[layer].Add(new TileInstance(i + offsetI, j + offsetJ, tileId));
                }
            }
        }

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, RedrawTarget.All), lastSelectedSectionsSC.defaultDirtyFlag, () =>
        {
            foreach (MapSectionLayer layer in lastSelectedSectionsSC.selectedMapSection.layers)
            {
                layer.ChangeTileGrid(tileInstances[layer]);
            }

            foreach (Instance instance in lastSelectedSectionsSC.selectedMapSection.instances)
            {
                if (instance.pos.y >= gridRect.i1 * TS && instance.pos.y <= gridRect.i2 * TS &&
                    instance.pos.x >= gridRect.j1 * TS && instance.pos.x <= gridRect.j2 * TS)
                {
                    instance.pos.y += offsetI * TS;
                    instance.pos.x += offsetJ * TS;
                }
            }

            gridRect.IncIJ(offsetI, offsetJ);

            context.FireEvent(EditorEvent.SelectedTileChange, lastSelectedSectionsSC);
        });
    }

    [Script("rt", "Remove transparent tiles")]
    public void RemoveTransparentTiles(string[] args)
    {
        HashSet<int> removedIds = new();
        foreach (Tile tile in tileset.GetAllTiles())
        {
            if (tile.id == Tile.TransparentTileId) continue;
            Color[,] colors = tileset.GetColorsFromTileHash(tile.hash, false);
            if (colors.All(color => color.A == 0) && tile.id != Tile.TransparentTileId)
            {
                removedIds.Add(tile.id);
            }
        }

        HashSet<int> actualRemovedIds = RemoveTilesWithChecks(removedIds);

        if (actualRemovedIds.Count == 0)
        {
            Prompt.ShowMessage("No unused transparent tiles were found or removed.");
            return;
        }

        foreach (MapSection mapSection in GetAllSections())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        if (removedIds.Contains(layer.tileGrid[i, j]))
                        {
                            layer.tileGrid[i, j] = Tile.TransparentTileId;
                        }
                    }
                }
            }
        }

        ForceSaveAll();
        Prompt.ShowMessage("Removed " + removedIds.Count + " transparent tiles.");
    }

    [Script("rtt", "Replace transparent tiles")]
    public void ReplaceTransparentTiles(string[] args)
    {
        foreach (MapSection mapSection in GetAllSections())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (tileId == 6879)
                        {
                            layer.ChangeTileGrid([new TileInstance(i, j, Tile.TransparentTileId)]);
                        }
                    }
                }
            }
        }

        RedrawAll();
        ForceSaveAll();
    }
}
