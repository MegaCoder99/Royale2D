using Editor;
using Shared;

namespace MapEditor;

public class TileClumpsSC : StateComponent
{
    public State state;
    public SectionsSC lastSelectedSectionsSC => state.lastSelectedSectionsSC;

    public TrackableList<TileClump> tileClumps { get => TrListGet<TileClump>(changeEvent: EditorEvent.TileClumpChange); set => TrListSet(value); }
    // Potential means it's the tile id that is the top left of the clump that COULD be a tile clump in the section grid, but not guaranteed
    public Dictionary<int, HashSet<TileClump>> cachedTileIdToPotentialClumps = new();
    public void UpdateTileClumpCache()
    {
        cachedTileIdToPotentialClumps.Clear();
        foreach (TileClump tileClump in tileClumps)
        {
            int topLeftTileId = tileClump.tileIds[0, 0];
            if (!cachedTileIdToPotentialClumps.ContainsKey(topLeftTileId))
            {
                cachedTileIdToPotentialClumps[topLeftTileId] = new HashSet<TileClump>();
            }
            cachedTileIdToPotentialClumps[topLeftTileId].Add(tileClump);
        }
    }
    public GridCoords? selectedTileClumpTopLeft { get => TrGet<GridCoords?>(); set => TrSet(value); }
    public TileClump? selectedTileClump { get => TrGet<TileClump?>(); set => TrSet(value, [nameof(canRemoveTileClump), nameof(showSelectedTileClump)]); }
    public bool canAddTileClump => lastSelectedSectionsSC.selectedTileCoords.Count > 1;
    public bool canRemoveTileClump => selectedTileClump != null;
    public bool showSelectedTileClump => selectedTileClump != null;
    public bool isDirty { get => TrGet<bool>(); set => TrSet(value, [nameof(tileClumpLabel)]); }
    public string tileClumpLabel => "Tile Clumps" + (isDirty ? "*" : "");

    public TileClumpsSC(EditorContext context, State state, List<TileClumpModel> tileClumps) : base(context)
    {
        this.state = state;
        this.tileClumps = new(tileClumps.Select(tc => new TileClump(context, tc)));
        UpdateTileClumpCache();
    }

    public List<TileClumpModel> ToModel()
    {
        return tileClumps.Select(tc => tc.ToModel()).ToList();
    }

    public void Save(MapWorkspace workspace, bool forceSave)
    {
        if (isDirty || forceSave)
        {
            workspace.SaveTileClumps(ToModel());
            isDirty = false;
        }
    }

    public void EditorEventHandler(EditorEvent editorEvent, StateComponent firer)
    {
        if (editorEvent == EditorEvent.SelectedTileChange && firer is SectionsSC sectionsSC)
        {
            // If the whole clump is selected, select that clump.
            TileClump? foundTileClump = null;
            int?[,] tileIdGrid = sectionsSC.GetSelectedTileIdGrids(false).GetTop();
            if (tileIdGrid.GetLength(0) > 0 && tileIdGrid.GetLength(1) > 0 && tileIdGrid[0, 0] != null && (tileIdGrid.GetLength(0) > 1 || tileIdGrid.GetLength(1) > 1))
            {
                int topLeftTileId = tileIdGrid[0, 0]!.Value;
                HashSet<TileClump> potentialClumps = cachedTileIdToPotentialClumps.GetValueOrDefault(topLeftTileId, []);
                foreach (TileClump clump in potentialClumps)
                {
                    if (clump != null && clump.CheckIfClumpMatches(tileIdGrid, 0, 0))
                    {
                        foundTileClump = clump;
                        break;
                    }
                }
            }
            if (foundTileClump != null)
            {
                selectedTileClump = foundTileClump;
                (int minI, int minJ) = sectionsSC.GetMinSelectionIJs();
                selectedTileClumpTopLeft = new GridCoords(minI, minJ);
            }
            else if (selectedTileClump != null)
            {
                // We deliberately don't clear out the tile clump if the user selects tiles inside that clump after initially selecting the clump.
                // The reason is to facilitate assigning of subsections to the tile clump, without having to do something complex like build out a whole new canvas in a popup window
                IEnumerable<int> tileClumpTileIds = selectedTileClump.tileIds.ToList().Distinct();
                IEnumerable<int?> selectionTileIds = tileIdGrid.ToList().Distinct();
                bool isSelectionSubset = selectionTileIds.All(tid => tid != null && tileClumpTileIds.Contains(tid.Value));
                if (!isSelectionSubset)
                {
                    selectedTileClump = null;
                }
            }

            QueueOnPropertyChanged(nameof(canAddTileClump));
        }
        if (editorEvent == EditorEvent.TileClumpChange)
        {
            QueueGenericAction(UpdateTileClumpCache);
        }
    }

    public HashSet<TileClump>? GetPotentialTileClumps(Tile tile)
    {
        return cachedTileIdToPotentialClumps.ContainsKey(tile.id) ? cachedTileIdToPotentialClumps[tile.id] : null;
    }

    public void SortTileClumpCommit()
    {
        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileClump, () =>
        {
            tileClumps.Sort(this);
        });
    }

    public void AddTileClumpCommit()
    {
        if (lastSelectedSectionsSC.selectedTileCoords.Count < 2)
        {
            return;
        }

        int?[,]? selectedTileIdGrid = lastSelectedSectionsSC.GetSelectedTileIdGrid(false);

        if (selectedTileIdGrid == null)
        {
            Prompt.ShowMessage("Multiple layers selected. Select one layer only.");
            return;
        }

        int[,] tileClumpGrid = new int[selectedTileIdGrid.GetLength(0), selectedTileIdGrid.GetLength(1)];

        for (int i = 0; i < selectedTileIdGrid.GetLength(0); i++)
        {
            for (int j = 0; j < selectedTileIdGrid.GetLength(1); j++)
            {
                if (selectedTileIdGrid[i, j] == null || selectedTileIdGrid[i, j] == Tile.TransparentTileId)
                {
                    Prompt.ShowError("Cannot create a tile clump with empty tiles");
                    return;
                }
                tileClumpGrid[i, j] = selectedTileIdGrid[i, j]!.Value;
            }
        }

        if (tileClumps.Any(tc => tc.CheckIfClumpMatches(tileClumpGrid, 0, 0)))
        {
            Prompt.ShowError("Tile clump already exists");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileClump, () =>
        {
            TileClump newTileClump = new(context, "NewTileClump", tileClumpGrid);
            tileClumps.Add(newTileClump);
            selectedTileClump = newTileClump;
            state.showTileClumps = true;
        });
    }

    public void RemoveTileClumpCommit()
    {
        if (selectedTileClump == null) return;

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileClump, () =>
        {
            // Check for references to this tile clump in other tile clumps
            foreach (TileClump tileClump in tileClumps)
            {
                if (tileClump == selectedTileClump) continue;
                if (tileClump.transformTileClumpNameCsv.Split(',').Contains(selectedTileClump.name))
                {
                    Prompt.ShowError($"Tile clump referenced by other tile clump \"{tileClump.name}\" in transformTileClumpNameCsv. Remove it there first.", "Cannot remove tile clump");
                    return;
                }
            }

            tileClumps.Remove(selectedTileClump);
            selectedTileClump = null;
        });
    }

    public void AddTileClumpSubsectionCommit()
    {
        if (selectedTileClump == null) return;

        if (lastSelectedSectionsSC.selectedTileCoords.Count < 1)
        {
            Prompt.ShowError("Select 1 or more cells within the tile clump.");
            return;
        }

        AddTcSubsectionDialog dialog = new();
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileClump, () =>
        {
            if (selectedTileClumpTopLeft == null) return; 

            List<GridCoords> newLayerAboveCells = [];

            GridCoords topLeftCell = selectedTileClumpTopLeft.Value;

            for (int i = 0; i < lastSelectedSectionsSC.selectedTileCoords.Count; i++)
            {
                GridCoords currentCell = lastSelectedSectionsSC.selectedTileCoords[i];
                GridCoords cellToAdd = new GridCoords(currentCell.i - topLeftCell.i, currentCell.j - topLeftCell.j);

                if (cellToAdd.i < 0 || cellToAdd.j < 0 || cellToAdd.i >= selectedTileClump.tileIds.GetLength(0) || cellToAdd.j >= selectedTileClump.tileIds.GetLength(1))
                {
                    Prompt.ShowError("Cells out of range of tile clump.");
                    return;
                }

                newLayerAboveCells.Add(cellToAdd);
            }

            TileClumpSubsection newTileClumpSubsection = new(context, dialog.name, newLayerAboveCells);
            selectedTileClump.subsections.Add(newTileClumpSubsection);
            selectedTileClump.selectedSubsection = newTileClumpSubsection;
        });
    }

    public void RemoveTileClumpSubsectionCommit()
    {
        if (selectedTileClump?.selectedSubsection == null)
        {
            Prompt.ShowError("Select a subsection from the list first.");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileClump, () =>
        {
            TileClumpSubsection toRemove = selectedTileClump.selectedSubsection;
            selectedTileClump.selectedSubsection = null;
            selectedTileClump.subsections.Remove(toRemove);
        });
    }
}
