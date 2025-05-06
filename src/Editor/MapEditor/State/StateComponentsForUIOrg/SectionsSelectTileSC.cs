using Editor;
using Shared;

namespace MapEditor;

// Contains state component code related to tile/cell selection
public partial class SectionsSC
{
    public TrackableList<GridCoords> selectedTileCoords => TrListGet<GridCoords>(onChangeCallback: OnSelectedTileCoordsChange, changeEvent: EditorEvent.SelectedTileChange);
    public void OnSelectedTileCoordsChange()
    {
        if (selectedTileCoords.Count != 0)
        {
            state.lastSelectedSectionsSC = this;
            otherSectionsSC?.selectedTileCoords.Clear();
        }
    }

    public int GetMaxJ() => selectedMapSection.firstLayer.tileGrid.GetLength(1) - 1;
    public int GetMaxI() => selectedMapSection.firstLayer.tileGrid.GetLength(0) - 1;
    public bool InBounds(GridCoords gridCoords) => gridCoords.i >= 0 && gridCoords.i <= GetMaxI() && gridCoords.j >= 0 && gridCoords.j <= GetMaxJ();

    #region canvas shortcuts
    public int zoom => canvas.zoom;
    public int scrollX => canvas.scrollX;
    public int scrollY => canvas.scrollY;
    public int mouseI => canvas.mouseI;
    public int mouseJ => canvas.mouseJ;
    public int mouseXInt => canvas.mouseXInt;
    public int mouseYInt => canvas.mouseYInt;
    public CanvasTool tool => canvas.tool;
    public bool RectInBounds(MyRect rect) => canvas.RectInBounds(rect);
    public void ChangeToDefaultTool() => canvas.ChangeToDefaultTool();
    #endregion

    public (int minI, int maxI, int minJ, int maxJ) GetMinMaxSelectionIJs()
    {
        int minI = selectedTileCoords.MinBy(tc => tc.i).i;
        int maxI = selectedTileCoords.MaxBy(tc => tc.i).i;
        int minJ = selectedTileCoords.MinBy(tc => tc.j).j;
        int maxJ = selectedTileCoords.MaxBy(tc => tc.j).j;
        return (minI, maxI, minJ, maxJ);
    }

    public (int minI, int minJ) GetMinSelectionIJs()
    {
        int minI = selectedTileCoords.MinBy(tc => tc.i).i;
        int minJ = selectedTileCoords.MinBy(tc => tc.j).j;
        return (minI, minJ);
    }

    public GridRect? GetSelectionGridRect()
    {
        if (selectedTileCoords.Count == 0) return null;
        (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();
        return new GridRect(minI, minJ, maxI, maxJ);
    }

    public MyRect? GetSelectionRect()
    {
        return GetSelectionGridRect()?.GetRect(TS);
    }

    public bool IsSelectionFullRect()
    {
        HashSet<int> selectedCoordHashes = GetSelectedTileCoordHash();
        (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();
        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                if (!selectedCoordHashes.Contains(GridCoords.GetHashCode(i, j)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public HashSet<int> GetSelectedTileCoordHash()
    {
        HashSet<int> selectedTileCoordHash = new();
        foreach (GridCoords cell in selectedTileCoords)
        {
            selectedTileCoordHash.Add(cell.GetHashCode());
        }
        return selectedTileCoordHash;
    }

    public SelectedTileIdGrids GetSelectedTileIdGrids(bool returnEmptyIfEmptySel)
    {
        Dictionary<int, int?[,]> layerToTileIdGrid = new();
        for (int i = 0; i < selectedMapSection.layers.Count; i++)
        {
            MapSectionLayer layer = selectedMapSection.layers[i];
            if (!layer.isSelected) continue;
            layerToTileIdGrid[i] = GetSelectedTileIdGridInternal(layer, returnEmptyIfEmptySel);
        }
        return new SelectedTileIdGrids(layerToTileIdGrid);
    }

    // Returns null if more than one layer selected, otherwise returns that layer's tile id grid
    public int?[,]? GetSelectedTileIdGrid(bool returnEmptyIfEmptySel)
    {
        MapSectionLayer? selectedLayer = selectedMapSection.GetSelectedLayer();
        if (selectedLayer == null) return null;
        return GetSelectedTileIdGridInternal(selectedLayer, returnEmptyIfEmptySel);
    }

    /// <summary>
    /// Returns 2D grid of selected tile ids (null if one of the slots not selected), bounded rectangular area
    /// </summary>
    /// <param name="returnEmptyIfEmptySel">If true, returns empty list if all selected tiles are empty, instead of grid with all null values</param>
    /// <returns>2D selection grid</returns>
    public int?[,] GetSelectedTileIdGridInternal(MapSectionLayer layer, bool returnEmptyIfEmptySel)
    {
        if (selectedTileCoords.Count == 0) return new int?[0, 0];

        (int minI, int maxI, int minJ, int maxJ) = GetMinMaxSelectionIJs();

        HashSet<int> selectedTileCoordHash = GetSelectedTileCoordHash();
        var selectedTileIds = new int?[maxI - minI + 1, maxJ - minJ + 1];
        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                if (!InBounds(new GridCoords(i, j))) continue;
                if (selectedTileCoordHash.Contains(GridCoords.GetHashCode(i, j)))
                {
                    selectedTileIds[i - minI, j - minJ] = layer.tileGrid[i, j];
                }
                else
                {
                    selectedTileIds[i - minI, j - minJ] = null;
                }
            }
        }

        if (returnEmptyIfEmptySel && !selectedTileIds.Any(id => id != null && id != Tile.TransparentTileId))
        {
            return new int?[0, 0];
        }

        return selectedTileIds;
    }

    public int GetTopTileId(int i, int j)
    {
        for (int index = selectedMapSection.layers.Count - 1; index >= 0; index--)
        {
            MapSectionLayer layer = selectedMapSection.layers[index];
            if (layer.isSelected && layer.tileGrid[i, j] != Tile.TransparentTileId)
            {
                return layer.tileGrid[i, j];
            }
        }
        return Tile.TransparentTileId;
    }

    public int GetTopTileLayer(int i, int j)
    {
        for (int index = selectedMapSection.layers.Count - 1; index >= 0; index--)
        {
            MapSectionLayer layer = selectedMapSection.layers[index];
            if (layer.isSelected && layer.tileGrid[i, j] != Tile.TransparentTileId)
            {
                return index;
            }
        }
        return 0;
    }

    public Tile GetTopTile(int i, int j)
    {
        return tileset.GetTileById(GetTopTileId(i, j));
    }

    public bool IsSelectAddModeOn()
    {
        return state.toggleAdditiveSelect || Helpers.ControlHeld();
    }

    public void SelectTilesCommitHelper(List<GridCoords> newSelectedCells, bool alwaysAddMode = false, bool removeIfAdded = false)
    {
        if (newSelectedCells.Count == 0) return;
        context.ApplyCodeCommit(new(RedrawFlag.Tooling, RedrawTarget.All), [], () =>
        {
            if (alwaysAddMode || IsSelectAddModeOn())
            {
                List<GridCoords> copiedSelectedTileCoords = new List<GridCoords>(selectedTileCoords);
                HashSet<GridCoords> selectedTileCoordsHash = new HashSet<GridCoords>(copiedSelectedTileCoords);
                foreach (GridCoords newSelectedCell in newSelectedCells)
                {
                    if (!selectedTileCoordsHash.Contains(newSelectedCell))
                    {
                        copiedSelectedTileCoords.Add(newSelectedCell);
                    }
                    else if (removeIfAdded)
                    {
                        copiedSelectedTileCoords.Remove(newSelectedCell);
                    }
                }
                selectedTileCoords.Replace(copiedSelectedTileCoords);
            }
            else
            {
                selectedTileCoords.Replace(newSelectedCells);
            }
        });
    }

    public void SelectTilesCommit(int mouseI, int mouseJ)
    {
        if (selectedMapSection == null) return;
        var gridCoords = new GridCoords(mouseI, mouseJ);
        if (!InBounds(gridCoords)) return;

        SelectTilesCommitHelper([gridCoords], false, true);
    }

    public void ShiftSelectCommit(int mouseI, int mouseJ)
    {
        if (selectedMapSection == null) return;
        var gridCoords = new GridCoords(mouseI, mouseJ);
        var newSelection = new List<GridCoords>();
        if (!InBounds(gridCoords)) return;

        var appended = new List<GridCoords>(selectedTileCoords);
        appended.Add(gridCoords);
        int minI = appended.Min(c => c.i);
        int minJ = appended.Min(c => c.j);
        int maxI = appended.Max(c => c.i);
        int maxJ = appended.Max(c => c.j);
        for (int i = minI; i <= maxI; i++)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                newSelection.Add(new GridCoords(i, j));
            }
        }

        SelectTilesCommitHelper(newSelection);
    }

    public void AltSelectCommit(GridCoords? lastMouseUpCell, int mouseI, int mouseJ)
    {
        if (selectedMapSection == null || lastMouseUpCell == null) return;

        var start = lastMouseUpCell.Value;
        var end = new GridCoords(mouseI, mouseJ);

        // Determine if the selection is a straight or diagonal line
        int deltaI = Math.Abs(end.i - start.i);
        int deltaJ = Math.Abs(end.j - start.j);

        if (deltaI != 0 && deltaJ != 0 && deltaI != deltaJ)
        {
            // Not a straight or perfectly diagonal line, so do nothing
            return;
        }

        var newSelection = new List<GridCoords>();

        int stepI = Math.Sign(end.i - start.i);
        int stepJ = Math.Sign(end.j - start.j);

        int length = Math.Max(deltaI, deltaJ);

        for (int k = 0; k <= length; k++)
        {
            newSelection.Add(new GridCoords(start.i + k * stepI, start.j + k * stepJ));
        }

        SelectTilesCommitHelper(newSelection, true);
    }

    public void FillSelectCommit(int startI, int startJ)
    {
        // Queue for BFS
        var queue = new Queue<GridCoords>();
        queue.Enqueue(new GridCoords(startI, startJ));

        // Set to keep track of visited coordinates
        var visited = new HashSet<GridCoords>();

        List<GridCoords> gridCoords = [];
        HashSet<int> selectedTileCoordsHash = GetSelectedTileCoordHash();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            // Check if current is within the bounds of the grid and if it is not selected
            if (InBounds(current) && !selectedTileCoordsHash.Contains(current.GetHashCode()))
            {
                gridCoords.Add(current);

                // Enqueue neighboring tiles
                queue.Enqueue(new GridCoords(current.i + 1, current.j));
                queue.Enqueue(new GridCoords(current.i - 1, current.j));
                queue.Enqueue(new GridCoords(current.i, current.j + 1));
                queue.Enqueue(new GridCoords(current.i, current.j - 1));
            }

            if (gridCoords.Count > 2500)
            {
                Console.WriteLine("Too many tiles to fill select. Aborting...");
                return;
            }
        }

        SelectTilesCommitHelper(gridCoords, true);
    }

    public void DragSelectCommit(int startI, int startJ, int endI, int endJ)
    {
        if (selectedMapSection == null) return;
        var gridCoords = new GridCoords(endI, endJ);
        if (!InBounds(gridCoords)) return;
        var newSelection = new List<GridCoords>();

        for (int i = startI; i <= endI; i++)
        {
            for (int j = startJ; j <= endJ; j++)
            {
                newSelection.Add(new GridCoords(i, j));
            }
        }

        SelectTilesCommitHelper(newSelection);
    }

    public void MoveSelectionCommit(int incX, int incY)
    {
        if (state.selectedMode == MapEditorMode.PaintTile)
        {
            MoveTileSelectionCommit(incX, incY);
            return;
        }
        else if (state.selectedMode == MapEditorMode.EditEntity)
        {
            if (MoveSelectedEntityCommit(incX, incY))
            {
                return;
            }
        }

        if (selectedTileCoords.Count == 0) return;
        var newSelection = new List<GridCoords>();
        if (incX != 0 || incY != 0)
        {
            foreach (GridCoords selectedTileCoord in selectedTileCoords)
            {
                GridCoords newCoord = new(selectedTileCoord.i + incY, selectedTileCoord.j + incX);
                if (InBounds(newCoord))
                {
                    newSelection.Add(newCoord);
                }
            }
        }

        RedrawWithUndo(() =>
        {
            selectedTileCoords.Replace(newSelection);
        });
    }

    public void UnselectAllTilesCommit()
    {
        if (tool is not SelectTool) return;
        RedrawWithUndo(() =>
        {
            if (selectedTileCoords.Count == 0 && (otherSectionsSC == null || otherSectionsSC.selectedTileCoords.Count == 0)) return;
            selectedTileCoords.Clear();
            if (otherSectionsSC != null) otherSectionsSC.selectedTileCoords.Clear();
        });
    }
}
