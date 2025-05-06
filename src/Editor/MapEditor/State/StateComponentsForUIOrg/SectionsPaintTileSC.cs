using Editor;
using Shared;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Input;

namespace MapEditor;

// Contains state component code related to the "Paint Tiles" editor mode.
// The tile paint mode is focused on painting (placing, erasing, moving, floodfill, etc) of tiles. It's a more "artistic" mode meant to craft game worlds visually.
public partial class SectionsSC
{
    static SelectedTileIdGrids? lastSelectedTileIdGridForClipboard;

    public void AddPaintTileModeHotkeys(HotkeyManager hotkeyManager)
    {
        hotkeyManager.AddHotkeys([
            new HotkeyConfig(Key.C, () => ToggleTilePlaceMode(false)),
            new HotkeyConfig(Key.X, () => ToggleTilePlaceMode(true)),
            new HotkeyConfig(Key.D, () => EraseSelectedTilesCommit(false)),
            new HotkeyConfig(Key.F, () => ChangeToEraseTool()),
            new HotkeyConfig(Key.G, () => ChangeToFillTool()),
            new HotkeyConfig(Key.R, () => state.toggleResizeTiles = !state.toggleResizeTiles),
            new HotkeyConfig(Key.A, () => state.toggleAdditiveSelect = !state.toggleAdditiveSelect),
            new HotkeyConfig(Key.Escape, ChangeToDefaultToolIfNotAlready),
            new HotkeyConfig(Key.C, HotkeyModifier.Control, () => CopyTiles()),
            new HotkeyConfig(Key.X, HotkeyModifier.Control, () => CutTiles()),
            new HotkeyConfig(Key.V, HotkeyModifier.Control, () => PasteTilesCommit()),
            new HotkeyConfig(Key.V, HotkeyModifier.Shift, () => PasteAndMergeTiles()),
            new HotkeyConfig(Key.Q, () => ChangeLayerIndexCommit(-1)),
            new HotkeyConfig(Key.E, () => ChangeLayerIndexCommit(1)),
            new HotkeyConfig(Key.Q, HotkeyModifier.Control, () => MoveSelectedTilesToLayerBelowCommit()),
            new HotkeyConfig(Key.E, HotkeyModifier.Control, () => MoveSelectedTilesToLayerAboveCommit()),
            new HotkeyConfig(Key.W, ToggleShowAllLayers),
            new HotkeyConfig(Key.H, () => FlipSelectionCommit(true)),
            new HotkeyConfig(Key.T, () => FlipSelectionCommit(false)),
        ], MapEditorMode.PaintTile);
    }

    #region tile paint tools radio buttons
    public bool selectTileToolChecked
    {
        get => tool is SelectTool;
        set { if (value) ChangeToDefaultTool(); }
    }

    public bool placeTileToolChecked
    {
        get => tool is PlaceTileTool;
        set
        {
            if (!value) return;
            SelectedTileIdGrids tilesToPlace = state.lastSelectedSectionsSC.GetSelectedTileIdGrids(true);
            if (tilesToPlace.IsEmpty())
            {
                Prompt.ShowError("No tiles selected, or all selections were blank tiles. Please select at least one non-blank tile to place.");
                return;
            }
            ChangeToPlaceTool(tilesToPlace);
        }
    }

    public bool eraseTileToolChecked
    {
        get => tool is EraseTileTool;
        set { if (value) ChangeToEraseTool(); }
    }

    public bool fillTileToolChecked
    {
        get => tool is FillTileTool;
        set
        {
            if (!value) return;
            ChangeToFillTool();
        }
    }

    public void OnPaintToolRadioPropertyChangeds()
    {
        OnPropertyChanged(nameof(selectTileToolChecked));
        OnPropertyChanged(nameof(placeTileToolChecked));
        OnPropertyChanged(nameof(eraseTileToolChecked));
        OnPropertyChanged(nameof(fillTileToolChecked));
    }
    #endregion

    public void ToggleShowAllLayers()
    {
        context.ApplyCodeCommit(new(RedrawFlag.Container, redrawTarget), [], () =>
        {
            foreach (MapSectionLayer layer in selectedMapSection.layers)
            {
                layer.isSelected = true;
            }
        });
    }

    public void ToggleTilePlaceMode(bool isMove)
    {
        if (tool is not PlaceTileTool)
        {
            SelectedTileIdGrids tilesToPlace = GetSelectedTileIdGrids(true);
            if (tilesToPlace.IsEmpty()) return;
            if (isMove)
            {
                EraseSelectedTilesCommit(true);
            }
            ChangeToPlaceTool(tilesToPlace);
        }
        else
        {
            ChangeToDefaultTool();
        }
    }

    public void ChangeToDefaultToolIfNotAlready()
    {
        if (tool is not SelectTool)
        {
            ChangeToDefaultTool();
        }
    }

    public void ChangeToPlaceTool(SelectedTileIdGrids tilesToPlace)
    {
        if (tilesToPlace.IsEmpty()) return;
        PlaceTileTool mapCanvasPlaceTileTool = new(CreatePreviewTilesDrawer(tilesToPlace), tilesToPlace);
        ChangeTool(mapCanvasPlaceTileTool);
    }

    public void ChangeToEraseTool()
    {
        ChangeTool(new EraseTileTool());
    }

    public void ChangeToFillTool()
    {
        int?[,] tilesToFill = GetSelectedTileIdGrids(false).GetTop();
        if (tilesToFill.GetLength(0) == 0)
        {
            Prompt.ShowError("No tiles selected, or all selections were blank tiles. Please one non-blank tile to fill.");
            return;
        }
        if (tilesToFill.GetLength(0) != 1 || tilesToFill.GetLength(1) != 1 || tilesToFill[0, 0] == null)
        {
            Prompt.ShowError("Fill tool currently only supports 1 tile as the fill tile.");
            return;
        }
        ChangeTool(new FillTileTool(tilesToFill[0, 0]!.Value));
    }

    public void PlacePreviewTilesCommit(SelectedTileIdGrids tileIdsToPlace)
    {
        PlaceTilesCommit(mouseI, mouseJ, tileIdsToPlace, false);
    }

    public void PlaceTilesCommit(int destI, int destJ, SelectedTileIdGrids tileIdsToPlace, bool selectPlaced, bool placeTransparentTiles = false)
    {
        if (tileIdsToPlace.IsEmpty()) return;

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
        {
            Action? postSelectTilesAction = null;
            foreach ((int index, int?[,] tileIdGrid) in tileIdsToPlace.layerToTileIdGrid)
            {
                MapSectionLayer? layer = selectedMapSection.layers.SafeGet(index);

                // If we copied from a single layer and are pasting to a single layer, just paste to that layer
                if (tileIdsToPlace.singleLayer && selectedMapSection.GetSelectedLayer() != null)
                {
                    layer = selectedMapSection.GetSelectedLayer();
                }

                if (layer == null)
                {
                    // IMPROVE a more elegant way of handling this for usability
                    Prompt.ShowMessage($"Layer {index} does not exist on destination canvas. Not copying.");
                    continue;
                }

                List<GridCoords> gridCoordsToSelect = new List<GridCoords>();
                List<TileInstance> tileInstances = [];
                for (int i = 0; i < tileIdGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < tileIdGrid.GetLength(1); j++)
                    {
                        var destCoords = new GridCoords(destI + i, destJ + j);
                        if (!InBounds(destCoords)) continue;

                        int? tileIdToPlace = tileIdGrid[i, j];
                        if (tileIdToPlace != null && (placeTransparentTiles || tileIdToPlace != Tile.TransparentTileId))
                        {
                            tileInstances.Add(new TileInstance(destCoords.i, destCoords.j, tileIdToPlace.Value));
                            gridCoordsToSelect.Add(destCoords);
                        }
                    }
                }
                if (tileInstances.Count == 0) continue;
                if (layer.ChangeTileGrid(tileInstances, selectPlaced) && selectPlaced)
                {
                    postSelectTilesAction = () =>
                    {
                        selectedTileCoords.Replace(gridCoordsToSelect);
                    };
                }
            }

            postSelectTilesAction?.Invoke();
        });
    }

    public void CutTiles()
    {
        TilePixelsExportHelper(true, true);
    }

    public void CopyTiles()
    {
        TilePixelsExportHelper(false, true);
    }

    public void TilePixelsExportHelper(bool cut, bool toClipboard)
    {
        if (selectedTileCoords.Count == 0) return;

        (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();

        int index = 0;
        Bitmap? tileClumpBitmap = null;

        // Clipboard doesn't support transparent colors, it defaults those to white. So transparentBgColor allows us to use magenta if that's set as the canvas transparent bg color
        Color transparentBgColor = magentaBgColor ? Color.Magenta : Color.White;

        foreach (MapSectionLayer layer in selectedMapSection.layers)
        {
            if (index == 0)
            {
                tileClumpBitmap = GetTileClumpBitmap(layer, minI, maxI, minJ, maxJ, transparentBgColor);
            }
            else
            {
                Bitmap tempLayerBitmap = GetTileClumpBitmap(layer, minI, maxI, minJ, maxJ, Color.Transparent);
                using (Graphics g = Graphics.FromImage(tileClumpBitmap!))
                {
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.DrawImage(tempLayerBitmap, 0, 0);
                }
                tempLayerBitmap.Dispose();
            }
            index++;
        }

        lastSelectedTileIdGridForClipboard = GetSelectedTileIdGrids(false);
        if (lastSelectedTileIdGridForClipboard.IsEmpty() || tileClumpBitmap == null)
        {
            lastSelectedTileIdGridForClipboard = null;
            Prompt.ShowError("All tiles selected were transparent");
            return;
        }

        if (toClipboard)
        {
            BitmapHelpers.SetBitmapToClipboard(tileClumpBitmap);
            if (cut) EraseSelectedTilesCommit(true);
        }
        else
        {
            string savePath = FolderPath.GetDesktopRawFolderPath() + "/" + Guid.NewGuid() + ".png";
            tileClumpBitmap.Save(savePath);
            MessageBox.Show("Successfully saved to " + savePath);
        }

        tileClumpBitmap.Dispose();
    }

    public void PasteTilesCommit()
    {
        Bitmap? bitmap = BitmapHelpers.GetBitmapFromClipboard(out bool isSourceMapEditor);
        if (bitmap == null)
        {
            Prompt.ShowError("No image detected in clipboard");
            return;
        }

        if (isSourceMapEditor)
        {
            if (lastSelectedTileIdGridForClipboard != null)
            {
                // FYI this DOES place transparent tiles unlike some of the other tile copy/paste options
                PlaceTilesCommit(selectedTileCoords[0].i, selectedTileCoords[0].j, lastSelectedTileIdGridForClipboard, true, true);
            }
        }
        else
        {
            TilePixelsImportHelper(bitmap);
        }
    }

    public void TilePixelsImportHelper(Bitmap bitmap)
    {
        if (selectedTileCoords.Count != 1)
        {
            Prompt.ShowError("Please select one cell where you will import an image.");
            return;
        }

        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            // IMPROVE reconsider generic wording. Also the wording could be tailored to specific scenarios
            // For example, in this case it could say "Multiple layers selected. Select one layer only that you want to paste the clipboard/file image to."
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        int TS = tileset.TS;
        if (bitmap.Width % TS != 0 || bitmap.Height % TS != 0)
        {
            Prompt.ShowError("Image dimensions must be a multiple of " + TS);
            return;
        }

        if (bitmap.Width == 0 || bitmap.Height == 0)
        {
            Prompt.ShowError("Image has 0 width or height");
            return;
        }

        var lockedBitmap = new LockedBitmap(bitmap);
        lockedBitmap.LockBits();

        var importGridTileHashes = new string[bitmap.Height / TS, bitmap.Width / TS];

        for (int y = 0; y < bitmap.Height; y += TS)
        {
            for (int x = 0; x < bitmap.Width; x += TS)
            {
                string tileHash = tileset.GetTileHash(lockedBitmap, x, y);
                importGridTileHashes[y / TS, x / TS] = tileHash;
            }
        }

        int newTileCount = 0;
        (int minI, int minJ) = GetMinSelectionIJs();

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), [], () =>
        {
            HashSet<DirtyFlag> dirtyFlags = [defaultDirtyFlag];
            List<GridCoords> newSelectedTileCoords = [];
            ConcurrentDictionary<string, Tile?> cache = [];
            PaletteData paletteData = new PaletteData(state);

            for (int i = minI; i < minI + importGridTileHashes.GetLength(0); i++)
            {
                for (int j = minJ; j < minJ + importGridTileHashes.GetLength(1); j++)
                {
                    string newTileHash = importGridTileHashes[i - minI, j - minJ];
                    Tile? newTile = tileset.GetFirstTileByHash(newTileHash);
                    if (newTile == null)
                    {
                        newTile = tilesetSC.AddNewTile(newTileHash);
                        newTileCount++;
                        dirtyFlags.Add(DirtyFlag.Tile);
                    }

                    selectedLayer.ChangeTileGrid([new TileInstance(i, j, newTile.id)]);
                    newSelectedTileCoords.Add(new GridCoords(i, j));
                }
            }
            selectedTileCoords.Replace(newSelectedTileCoords);
            return new ExtraCommitData(null, dirtyFlags);
        });

        bitmap.Dispose();
        if (newTileCount > 0)
        {
            Prompt.ShowMessage("Imported " + newTileCount + " new tiles");
        }
    }

    private Bitmap GetTileClumpBitmap(MapSectionLayer layer, int minI, int maxI, int minJ, int maxJ, Color transparentColor)
    {
        Bitmap tileClumpBitmap = new Bitmap((maxJ - minJ + 1) * 8, (maxI - minI + 1) * 8);
        var lockBitmap = new LockedBitmap(tileClumpBitmap);
        lockBitmap.LockBits();

        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                Tile tile = tileset.GetTileById(layer.tileGrid[i, j]);
                if (tile == null) continue;
                Color[,] pixels = tileset.GetColorsFromTileHash(tile.hash, false);

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        int drawX = x + ((j - minJ) * 8);
                        int drawY = y + ((i - minI) * 8);

                        if (pixels[y, x].A == 0)
                        {
                            lockBitmap.SetPixel(drawX, drawY, transparentColor);
                        }
                        else
                        {
                            lockBitmap.SetPixel(drawX, drawY, pixels[y, x]);
                        }
                    }
                }
            }
        }

        lockBitmap.UnlockBits();
        return tileClumpBitmap;
    }

    private bool IsBitmapEntirelyTransparent(Bitmap bitmap, Color transparentBgColor)
    {
        var lockBitmap = new LockedBitmap(bitmap);
        lockBitmap.LockBits();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (lockBitmap.GetPixel(x, y) != transparentBgColor)
                {
                    lockBitmap.UnlockBits();
                    return false;
                }
            }
        }
        lockBitmap.UnlockBits();
        return true;
    }

    // IMPROVE port this for use with external clipboard image? If not, at least show error if trying to do so
    public void PasteAndMergeTiles()
    {
        if (selectedTileCoords.Count != 1)
        {
            Prompt.ShowError("Please select one cell where you will paste and merge tiles.");
            return;
        }

        if (lastSelectedTileIdGridForClipboard == null)
        {
            Prompt.ShowError("Nothing in clipboard.");
            return;
        }

        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        if (lastSelectedTileIdGridForClipboard.layerToTileIdGrid.Count > 1)
        {
            Prompt.ShowError("Selection consists of multiple layers of tiles. This feature will only work with one layer of tile selections.");
            return;
        }

        int?[,] tileIdGrid = lastSelectedTileIdGridForClipboard.layerToTileIdGrid.First().Value;

        (int minI, int minJ) = GetMinSelectionIJs();

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), [], () =>
        {
            List<GridCoords> newSelectedTileCoords = [];
            int subsitutionCount = 0;
            HashSet<int> usedOldTileIds = [];
            Dictionary<int, int> tileIdsToReplace = [];
            for (int i = minI; i < minI + tileIdGrid.GetLength(0); i++)
            {
                for (int j = minJ; j < minJ + tileIdGrid.GetLength(1); j++)
                {
                    newSelectedTileCoords.Add(new GridCoords(i, j));

                    int oldTileId = selectedLayer.tileGrid[i, j];
                    int? newTileId = tileIdGrid[i - minI, j - minJ];
                    if (newTileId == null || newTileId == Tile.TransparentTileId || oldTileId == newTileId.Value)
                    {
                        continue;
                    }

                    selectedLayer.ChangeTileGrid([new TileInstance(i, j, newTileId.Value)]);

                    subsitutionCount++;
                    tileIdsToReplace[oldTileId] = newTileId.Value;

                    if (!selectedMapSection.isDirty) selectedMapSection.isDirty = true;

                    if (oldTileId == Tile.TransparentTileId) continue;
                    if (usedOldTileIds.Contains(oldTileId)) continue;
                    usedOldTileIds.Add(oldTileId);

                    foreach ((MapSection mapSection, SectionsSC sectionsSC) in state.GetAllSectionsWithSectionsSC())
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
                                        if (layer.ChangeTileGrid([new TileInstance(k, l, newTileId.Value)]))
                                        {
                                            if (!mapSection.isDirty) mapSection.isDirty = true;
                                            subsitutionCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            selectedTileCoords.Replace(newSelectedTileCoords);

            HashSet<DirtyFlag> replaceRefDirtyFlags = tileset.ReplaceTileForeignKeyRefs(tileIdsToReplace, state);
            string foreignKeyMessage = replaceRefDirtyFlags.Count > 0 ? ". ATTENTION: some tile id references had to change in other data models (clumps, animations, etc)." : "";

            if (subsitutionCount > 0)
            {
                Prompt.ShowMessage("Replaced " + subsitutionCount + " tiles throughout all sections" + foreignKeyMessage);
            }
            else
            {
                // IMPROVE if this is desirable, find a way to see if there are pending undo nodes, and if so document that function
                // Prompt.ShowMessage("No changes. The destination tiles are the same as source.");
            }
            return new ExtraCommitData(null, replaceRefDirtyFlags);
        });
    }

    public void ResizeMouseUpCommit(List<TileInstance> tileChanges)
    {
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
        {
            selectedLayer.ChangeTileGrid(tileChanges);
            selectedTileCoords.Clear();
        });
    }

    public void FillTilesCommit(int startI, int startJ, int tileIdToFill)
    {
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        // Get the target tile ID to be replaced
        int targetTileId = selectedLayer.tileGrid[startI, startJ];

        // If the selected tile ID is the same as the target tile ID, no action is required
        if (tileIdToFill == targetTileId) return;

        // Queue for BFS
        var queue = new Queue<GridCoords>();
        queue.Enqueue(new GridCoords(startI, startJ));

        // Set to keep track of visited coordinates
        var visited = new HashSet<GridCoords>();

        List<TileInstance> tileInstances = [];

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            // Check if current is within the bounds of the grid and if it matches the targetTileId
            if (current.i >= 0 && current.i < selectedLayer.tileGrid.GetLength(0) && current.j >= 0 && current.j < selectedLayer.tileGrid.GetLength(1) && selectedLayer.tileGrid[current.i, current.j] == targetTileId)
            {
                tileInstances.Add(new TileInstance(current.i, current.j, tileIdToFill));

                // Enqueue neighboring tiles
                queue.Enqueue(new GridCoords(current.i + 1, current.j));
                queue.Enqueue(new GridCoords(current.i - 1, current.j));
                queue.Enqueue(new GridCoords(current.i, current.j + 1));
                queue.Enqueue(new GridCoords(current.i, current.j - 1));
            }
        }

        if (tileInstances.Count > 0)
        {
            context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
            {
                selectedLayer.ChangeTileGrid(tileInstances);
            });
        }
    }

    public void EraseTilesCommit(int destI, int destJ)
    {
        List<TileInstance> tileInstances = [new TileInstance(destI, destJ, Tile.TransparentTileId)];
        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
        {
            foreach (MapSectionLayer layer in selectedMapSection.layers)
            {
                if (!layer.isSelected) continue;
                layer.ChangeTileGrid(tileInstances);
            }
        });
    }

    public void EraseSelectedTilesCommit(bool unselectTiles)
    {
        if (selectedTileCoords.Count == 0) return;

        List<TileInstance> tileInstances = [];
        
        foreach (GridCoords selectedTileCoord in selectedTileCoords)
        {
            tileInstances.Add(new TileInstance(selectedTileCoord.i, selectedTileCoord.j, GetDefaultTileId()));
        }

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
        {
            foreach (MapSectionLayer layer in selectedMapSection.layers)
            {
                if (!layer.isSelected) continue;
                layer.ChangeTileGrid(tileInstances);
            }
            if (unselectTiles) selectedTileCoords.Clear();
        });
    }

    public void MoveTileSelectionCommit(int incX, int incY)
    {
        if (selectedTileCoords.Count == 0) return;

        if (Helpers.ControlHeld())
        {
            incX *= 10;
            incY *= 10;
        }

        if (incX != 0 || incY != 0)
        {
            (int minI, int minJ) = GetMinSelectionIJs();

            SelectedTileIdGrids tileIdsToPlace = GetSelectedTileIdGrids(false);

            EraseSelectedTilesCommit(true);
            PlaceTilesCommit(minI + incY, minJ + incX, tileIdsToPlace, true);
            undoManager?.MergeLastNUndoGroups(2);
        }
    }

    public void MoveSelectedTilesToLayerAboveCommit()
    {
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        int layerAboveIndex = selectedMapSection.layers.IndexOf(selectedLayer) + 1;
        if (layerAboveIndex >= selectedMapSection.layers.Count)
        {
            Prompt.ShowError("Cannot move tiles to a layer above the top layer");
            return;
        }
        MoveSelectedTilesToNewLayerCommit(layerAboveIndex);
    }

    public void MoveSelectedTilesToLayerBelowCommit()
    {
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        int layerBelowIndex = selectedMapSection.layers.IndexOf(selectedLayer) - 1;
        if (layerBelowIndex < 0)
        {
            Prompt.ShowError("Cannot move tiles to a layer below the bottom layer");
            return;
        }
        MoveSelectedTilesToNewLayerCommit(layerBelowIndex);
    }

    public void MoveSelectedTilesToNewLayerCommit(int layerIndex)
    {
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        MapSectionLayer? newLayer = null;
        if (selectedMapSection.layers.IndexOf(selectedLayer) != layerIndex)
        {
            newLayer = selectedMapSection.layers[layerIndex];
        }
        if (newLayer == null) return;
        if (selectedTileCoords.Count == 0) return;

        List<TileInstance> tileInstancesOld = [];
        List<TileInstance> tileInstancesNew = [];
        foreach (GridCoords selectedTileCoord in selectedTileCoords)
        {
            int selectedTileId = selectedLayer.tileGrid[selectedTileCoord.i, selectedTileCoord.j];
            tileInstancesOld.Add(new TileInstance(selectedTileCoord.i, selectedTileCoord.j, Tile.TransparentTileId));
            if (selectedTileId != Tile.TransparentTileId)
            {
                tileInstancesNew.Add(new TileInstance(selectedTileCoord.i, selectedTileCoord.j, selectedTileId));
            }
        }

        if (tileInstancesNew.Count == 0) return;

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), defaultDirtyFlag, () =>
        {
            selectedLayer.ChangeTileGrid(tileInstancesOld);
            newLayer.ChangeTileGrid(tileInstancesNew);
        });
    }

    public void FlipSelectionCommit(bool isHorizontal)
    {
        // Note: flipping doesn't currently work on non-rectangular, disjoint selections

        if (selectedTileCoords.Count <= 0) return;

        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();

        int?[,] newSelectedTileIds = Helpers.Create2DArray<int?>((maxI - minI) + 1, (maxJ - minJ) + 1, null);

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, redrawTarget), [DirtyFlag.Tile], () =>
        {
            int newCount = 0;
            for (int i = minI; i <= maxI; i++)
            {
                for (int j = minJ; j <= maxJ; j++)
                {
                    bool isNew;
                    Tile tileData = state.tileset.GetClonedTileById(selectedLayer.tileGrid[i, j]);
                    Tile hFlipped = isHorizontal ? tileset.FlipHorizontal(tileData, state.tilesetSC, out isNew) : tileset.FlipVertical(tileData, state.tilesetSC, out isNew);
                    newSelectedTileIds[i - minI, j - minJ] = hFlipped.id;
                    if (isNew) newCount++;
                }
            }

            if (isHorizontal)
            {
                newSelectedTileIds.FlipHorizontal();
            }
            else
            {
                newSelectedTileIds.FlipVertical();
            }

            selectedTileCoords.Clear();

            if (newCount > 0)
            {
                // IMPROVE prompt user beforehand, inform them it is undo'able, and maybe an easier way to see that tiles were added to the tileset
                Prompt.ShowMessage("Flip tile operation created new tiles. " + newCount + " new tiles were created.");
            }
        });

        int layerIndex = selectedMapSection.layers.IndexOf(selectedLayer);
        ChangeToPlaceTool(new SelectedTileIdGrids(layerIndex, newSelectedTileIds));
    }

    public Drawer CreatePreviewTilesDrawer(SelectedTileIdGrids tileIdsToPlace)
    {
        if (tileIdsToPlace.IsEmpty())
        {
            throw new Exception("DrawPreviewTiles: no tiles");
        }

        BitmapDrawer drawer = new(tileIdsToPlace.cols * TS, tileIdsToPlace.rows * TS);

        foreach ((int _, int?[,] tileIdGrid) in tileIdsToPlace.layerToTileIdGrid)
        {
            for (int i = 0; i < tileIdGrid.GetLength(0); i++)
            {
                for (int j = 0; j < tileIdGrid.GetLength(1); j++)
                {
                    int? tileId = tileIdGrid[i, j];
                    if (tileId != null)
                    {
                        Tile tile = state.tileset.GetTileById(tileId.Value);
                        Drawer tileToDraw = state.tileset.GetDrawerFromTileHash(tile.hash);
                        drawer.DrawImage(tileToDraw, j * TS, i * TS);
                    }
                }
            }
        }

        return drawer;
    }
}
