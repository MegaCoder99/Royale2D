using Editor;
using Shared;

namespace MapEditor;

public enum MapEditorMode
{
    PaintTile,
    EditTileData,
    EditEntity,
}

public partial class State : StateComponent, IEditorState
{
    public TilesetSC tilesetSC;
    public Tileset tileset => tilesetSC.tileset;
    public TileAnimationsSC tileAnimationSC { get; set; }
    public TileClumpsSC tileClumpSC { get; set; }
    public SectionsSC mapSectionsSC { get; set; }
    public SectionsSC scratchSectionsSC { get; set; }
    public SectionsSC lastSelectedSectionsSC { get => TrGet<SectionsSC>(); set => TrSet(value); }
    public SelectedTileDisplaySC selectedCellSC { get; set; }

    public List<string> instanceTypes { get; set; } = new(InstanceTypes.all.Select(it => it.name));
    public List<string> entranceDirs => EntranceData.directions;
    public List<string> zoneTypes { get; set; } = new(ZoneTypes.all.Select(zt => zt.name));
    public string selectedInstanceType { get => TrGet<string>(); set => TrSet(value); }
    public string selectedCreateZoneType { get => TrGet<string>(); set => TrSet(value); }
    public MapEditorMode selectedMode { get => TrGet<MapEditorMode>(); set => TrSetAll(value, modePropNameList, null, OnSelectedModeChange); }
    public string workspaceLabel => "Workspace: " + Config.main.workspacePath;
    public List<string> recentWorkspaces => Config.main.recentWorkspaces;
    public void OnSelectedModeChange(MapEditorMode oldValue, MapEditorMode newValue)
    {
        if (newValue == MapEditorMode.EditTileData)
        {
            //showTileHitboxes = true;
        }
        lastSelectedSectionsSC.canvas.Focus();
        QueueGenericAction(() =>
        {
            if (selectedMode != MapEditorMode.PaintTile)
            {
                mapSectionsSC.ChangeToDefaultToolIfNotAlready();
            }
        });
    }
    public string mouseClickedText { get; set; } = "";
    public string colorClickedText { get; set; } = "";

    public bool showGrid { get => TrGet<bool>(); set => TrSet(value); }
    public bool showEntities { get => TrGet<bool>(); set => TrSetC(value, OnShowEntitiesChange); }
    public bool showTileHitboxes { get => TrGet<bool>(); set => TrSet(value); }
    public bool showTileZIndices { get => TrGet<bool>(); set => TrSet(value); }
    public bool showTileAnimations { get => TrGet<bool>(); set => TrSet(value); }
    public bool showTileClumps { get => TrGet<bool>(); set => TrSet(value); }
    public bool showSameTiles { get => TrGet<bool>(); set => TrSet(value); }
    public string showTilesWithTagsText { get => TrGetD(""); set => TrSet(value); }
    public int showRarelyUsedTilesCount { get => TrGet<int>(); set => TrSet(value); }
    public Dictionary<Tile, int> tileToCountForSrScript = [];

    public void OnShowEntitiesChange(bool oldValue, bool newValue)
    {
        if (!newValue)
        {
            mapSectionsSC.selectedMapSection.selectedInstance = null;
            mapSectionsSC.selectedMapSection.selectedZone = null;
        }
    }
    public bool drawIndoorMappingData { get => TrGetD(false); set => TrSet(value); }
    public bool showCameraBounds { get => TrGet<bool>(); set => TrSet(value); }
    public bool showTopLeftCameraBounds { get => TrGet<bool>(); set => TrSet(value); }
    public string editTileDataHotkeyDisplay => HelpText.EditTileDataModeHotkeys;
    public string paintTileHotkeyDisplay => HelpText.TilePaintModeHotkeys;
    public string editEntityHotkeyDisplay => HelpText.EditEntityModeHotkeys;
    public string tileTags => string.Join('\n', TileTags.predefinedTileTags);
    public bool hasPredefinedTileTags => TileTags.predefinedTileTags.Length > 0;

    #region editor mode radio buttons
    public bool paintTileModeChecked
    {
        get => selectedMode == MapEditorMode.PaintTile;
        set => selectedMode = value ? MapEditorMode.PaintTile : selectedMode;
    }

    public bool editTileDataModeChecked
    {
        get => selectedMode == MapEditorMode.EditTileData;
        set => selectedMode = value ? MapEditorMode.EditTileData : selectedMode;
    }

    public bool editEntityModeChecked
    {
        get => selectedMode == MapEditorMode.EditEntity;
        set => selectedMode = value ? MapEditorMode.EditEntity : selectedMode;
    }

    public string[] modePropNameList = [nameof(paintTileModeChecked), nameof(editTileDataModeChecked), nameof(editEntityModeChecked)];
    #endregion

    public bool canSaveAll => 
        mapSectionsSC.isDirty ||
        scratchSectionsSC.isDirty ||
        tilesetSC.isDirty ||
        tileAnimationSC.isDirty ||
        tileClumpSC.isDirty;

    public bool canUndo => undoManager?.canUndo == true;
    public bool canRedo => undoManager?.canRedo == true;
    public bool canAddZoneToSelection => mapSectionsSC.selectedTileCoords.Count >= 10;

    public ScriptManager scriptManager;

    public const int ScratchCanvasWidth = 400;
    private bool _hideScratchCanvas;
    public bool hideScratchCanvas
    {
        get => _hideScratchCanvas;
        set
        {
            _hideScratchCanvas = value;
            // +20 accounts for scrollbar
            mapSectionsSC.canvas.Resize(
                UISizes.main.mapCanvasWidth + (value ? UISizes.main.scratchCanvasWidth + 20 : 0), 
                mapSectionsSC.canvas.canvasHeight, 
                mapSectionsSC.canvas.totalWidth, 
                mapSectionsSC.canvas.totalHeight);
            OnPropertyChanged(nameof(hideScratchCanvas));
            Redraw(RedrawData.ToolingAll);
        }
    }
    public int mapCanvasWidth => mapSectionsSC.canvas.canvasWidth;

    // Examples of properties that aren't trackables - not undo'able because they don't meaningfully change state or even the display, not warranting undo
    private bool _toggleResizeTiles;
    public bool toggleResizeTiles { get => _toggleResizeTiles; set { _toggleResizeTiles = value; OnPropertyChanged(nameof(toggleResizeTiles)); } }
    private bool _toggleAdditiveSelect;
    public bool toggleAdditiveSelect { get => _toggleAdditiveSelect; set { _toggleAdditiveSelect = value; OnPropertyChanged(nameof(toggleAdditiveSelect)); } }

    public MapWorkspace workspace;

    public State(EditorContext context, MapWorkspace workspace, CanvasControl mapCanvasControl, CanvasControl scratchCanvasControl) : base(context)
    {
        this.workspace = workspace;

        tilesetSC = new(context, workspace.tileset);
        tileAnimationSC = new(context, this, workspace.tileAnimations);
        tileClumpSC = new(context, this, workspace.tileClumps);

        mapSectionsSC = new(this, false, mapCanvasControl);
        scratchSectionsSC = new(this, true, scratchCanvasControl);

        mapSectionsSC.otherSectionsSC = scratchSectionsSC;
        scratchSectionsSC.otherSectionsSC = mapSectionsSC;
        lastSelectedSectionsSC = mapSectionsSC;

        // CONFIG BOOKMARK (LOAD)

        if (mapSectionsSC.selectedMapSection != null)
        {
            mapSectionsSC.selectedMapSection.lastScrollX = WorkspaceConfig.main.lastMapCanvasScrollX;
            mapSectionsSC.selectedMapSection.lastScrollY = WorkspaceConfig.main.lastMapCanvasScrollY;
            mapSectionsSC.selectedMapSection.lastZoom = WorkspaceConfig.main.lastMapSectionZoom;
            for (int layerIndex = 0; layerIndex < mapSectionsSC.selectedMapSection.layers.Count; layerIndex++)
            {
                if (WorkspaceConfig.main.lastSelectedMapLayerIndices.Count > 0 && !WorkspaceConfig.main.lastSelectedMapLayerIndices.Contains(layerIndex))
                {
                    mapSectionsSC.selectedMapSection.layers[layerIndex].isSelected = false; // false if not, because true is the default 
                }
            }

            // If none selected (first time load or user messed with config.json) select all as default
            if (mapSectionsSC.selectedMapSection.layers.All(layer => !layer.isSelected))
            {
                foreach (MapSectionLayer layer in mapSectionsSC.selectedMapSection.layers)
                {
                    layer.isSelected = true;
                }
            }
        }

        if (scratchSectionsSC.selectedMapSection != null)
        {
            scratchSectionsSC.selectedMapSection.lastScrollX = WorkspaceConfig.main.lastScratchCanvasScrollX;
            scratchSectionsSC.selectedMapSection.lastScrollY = WorkspaceConfig.main.lastScratchCanvasScrollY;
            scratchSectionsSC.selectedMapSection.lastZoom = WorkspaceConfig.main.lastScratchCanvasZoom;
        }

        selectedCellSC = new SelectedTileDisplaySC(context, this);
        scriptManager = new(this);

        selectedInstanceType = instanceTypes[0];
        selectedMode = (MapEditorMode)WorkspaceConfig.main.lastMode;
        showGrid = WorkspaceConfig.main.showGrid;
        showTileHitboxes = WorkspaceConfig.main.showTileHitboxes;
        showTileZIndices = WorkspaceConfig.main.showTileZIndices;
        showTileAnimations = WorkspaceConfig.main.showTileAnimations;
        showTileClumps = WorkspaceConfig.main.showTileClumps;
        showSameTiles = WorkspaceConfig.main.showSameTiles;
        showEntities = WorkspaceConfig.main.showEntities;
        hideScratchCanvas = WorkspaceConfig.main.hideScratchSection;
    }

    public void RunScript(string script)
    {
        scriptManager.RunScript(script);
    }

    // Redraw literally everything that can be redrawn. This is very slow, so should only be done in initialization once, or maybe in one-off power scripts
    public void RedrawAll()
    {
        Redraw(new(RedrawFlag.All, RedrawTarget.All));
    }

    public void Redraw(RedrawData redrawData)
    {
        if (redrawData.redrawTarget == RedrawTarget.Map || redrawData.redrawTarget == RedrawTarget.All)
        {
            mapSectionsSC.Redraw(redrawData.redrawFlag);
        }
        if (redrawData.redrawTarget == RedrawTarget.Scratch || redrawData.redrawTarget == RedrawTarget.All)
        {
            scratchSectionsSC.Redraw(redrawData.redrawFlag);
        }
    }

    public void OnUndoRedo()
    {
        OnPropertyChanged(nameof(canSaveAll));
        OnPropertyChanged(nameof(canUndo));
        OnPropertyChanged(nameof(canRedo));
    }

    public void EditorEventHandler(EditorEvent editorEvent, StateComponent firer)
    {
        if (editorEvent == EditorEvent.SelectedTileChange && (firer as SectionsSC)?.isScratch != true)
        {
            QueueOnPropertyChanged(nameof(canAddZoneToSelection));
        }
        tileAnimationSC.EditorEventHandler(editorEvent, firer);
        tileClumpSC.EditorEventHandler(editorEvent, firer);
        selectedCellSC.EditorEventHandler(editorEvent);
    }

    public void SetDirty(DirtyFlag dirtyFlag)
    {
        if (dirtyFlag == DirtyFlag.Map)
        {
            mapSectionsSC.selectedMapSection.isDirty = true;
        }
        else if (dirtyFlag == DirtyFlag.Scratch)
        {
            scratchSectionsSC.selectedMapSection.isDirty = true;
        }
        else if (dirtyFlag == DirtyFlag.Tile)
        {
            tilesetSC.isDirty = true;
        }
        else if (dirtyFlag == DirtyFlag.TileAnimation)
        {
            tileAnimationSC.isDirty = true;
        }
        else if (dirtyFlag == DirtyFlag.TileClump)
        {
            tileClumpSC.isDirty = true;
        }
    }

    public bool ValidateBeforeSave()
    {
        if (tileClumpSC.tileClumps.HasDuplicate(tc => tc.name, out string? duplicateTileClumpName))
        {
            Prompt.ShowError($"Duplicate tile clump name: {duplicateTileClumpName}", "Validation failed");
            return false;
        }

        if (tileAnimationSC.tileAnimations.HasDuplicate(ta => ta.id, out int? duplicateTileAnimationId))
        {
            Prompt.ShowError($"Duplicate tile animation id: {duplicateTileAnimationId}", "Validation failed");
            return false;
        }

        if (!mapSectionsSC.GetEntranceZoneData().Validate(false))
        {
            return false;
        }

        return true;
    }

    public void SaveAll()
    {
        if (!ValidateBeforeSave()) return;

        RemoveUnusedTiles(["a"]);

        mapSectionsSC.Save(workspace, false);
        scratchSectionsSC.Save(workspace, false);
        tilesetSC.Save(workspace, false);
        tileAnimationSC.Save(workspace, false);
        tileClumpSC.Save(workspace, false);

        OnPropertyChanged(nameof(canSaveAll));
    }

    // Generally only used by scripts
    public void ForceSaveAll()
    {
        mapSectionsSC.Save(workspace, true);
        scratchSectionsSC.Save(workspace, true);
        tilesetSC.Save(workspace, true);
        tileAnimationSC.Save(workspace, true);
        tileClumpSC.Save(workspace, true);

        // When force saving, we generally don't care about setting dirty and undo state since they are for "power tool" operations,
        // and user should be aware their undo stack is being nuked. Were we not to do this, undo could get in a broken state
        context.undoManager?.Nuke();
    }

    public void Export()
    {
        FolderPath defaultPath = workspace.baseFolderPath.AppendFolder("exported");
        ExportDialog dialog = new();

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        bool result = new Exporter(this, dialog.selectedPath, dialog.exportOption).Export();
        if (!result)
        {
            return;
        }

        OptionsDialog optionsDialog = new("Exported successfully", $"Exported successfully to path:\n{dialog.selectedPath}", ["Open folder", "Close"]);
        optionsDialog.ShowDialog();
        if (optionsDialog.selectedButtonIndex == 0)
        {
            Prompt.OpenFolderInExplorer(dialog.selectedPath);
        }
    }

    public List<MapSection> GetAllSections()
    {
        List<MapSection> sections = [.. mapSectionsSC.mapSections, .. scratchSectionsSC.mapSections];
        return sections;
    }

    // If you need to get the parent along with the section, use this
    // REFACTOR can put SectionsSC in MapSection to avoid this
    public List<(MapSection, SectionsSC)> GetAllSectionsWithSectionsSC()
    {
        List<(MapSection, SectionsSC)> sections = [];
        foreach (MapSection section in mapSectionsSC.mapSections)
        {
            sections.Add((section, mapSectionsSC));
        }
        foreach (MapSection section in scratchSectionsSC.mapSections)
        {
            sections.Add((section, scratchSectionsSC));
        }
        return sections;
    }

    public void ShowCenterCameraBounds()
    {
        if (selectedMode != MapEditorMode.EditTileData)
        {
            showCameraBounds = !showCameraBounds;
            showTopLeftCameraBounds = false;
            Redraw(RedrawData.ToolingAll);
        }
    }

    public void ShowTopLeftCameraBounds()
    {
        if (selectedMode != MapEditorMode.EditTileData)
        {
            showTopLeftCameraBounds = !showTopLeftCameraBounds;
            showCameraBounds = false;
            Redraw(RedrawData.ToolingAll);
        }
    }

    public Dictionary<Tile, int> GetTileToCount()
    {
        Dictionary<Tile, int> tileToCount = [];
        foreach ((MapSection mapSection, SectionsSC sectionsSC) in GetAllSectionsWithSectionsSC())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (tileId == 0) continue;
                        Tile tile = tileset.GetTileById(tileId);
                        if (!tileToCount.ContainsKey(tile))
                        {
                            tileToCount[tile] = 0;
                        }
                        tileToCount[tile]++;
                    }
                }
            }
        }
        return tileToCount;
    }

    public record JumpIterationNode((MapSection, SectionsSC) sectionAndSC, MapSectionLayer layer, int i, int j);

    public void JumpToPlaceUsingTileId(bool previous)
    {
        if (lastSelectedSectionsSC.selectedTileCoords.Count != 1)
        {
            Prompt.ShowMessage("Select a single tile");
            return;
        }

        int tileId = lastSelectedSectionsSC.GetTopTileId(lastSelectedSectionsSC.selectedTileCoords[0].i, lastSelectedSectionsSC.selectedTileCoords[0].j);

        int startI = lastSelectedSectionsSC.selectedTileCoords[0].i;
        int startJ = lastSelectedSectionsSC.selectedTileCoords[0].j;
        MapSection startMapSection = lastSelectedSectionsSC.selectedMapSection;
        MapSectionLayer startLayer = lastSelectedSectionsSC.selectedMapSection.layers.First(l => l.isSelected);

        int jumpIterationStartIndex = 0;
        List<JumpIterationNode> jumpIterationNodes = [];
        foreach ((MapSection mapSection, SectionsSC sectionsSC) in GetAllSectionsWithSectionsSC())
        {
            foreach (MapSectionLayer layer in mapSection.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        jumpIterationNodes.Add(new JumpIterationNode((mapSection, sectionsSC), layer, i, j));
                        if (mapSection == startMapSection && layer == startLayer && i == startI && j == startJ)
                        {
                            jumpIterationStartIndex = !previous ? jumpIterationNodes.Count : jumpIterationNodes.Count - 2;
                        }
                    }
                }
            }
        }

        if (!previous)
        {
            for (int index = 0; index < jumpIterationNodes.Count; index++)
            {
                var jumpIterationNode = jumpIterationNodes[(index + jumpIterationStartIndex) % jumpIterationNodes.Count];

                if (jumpIterationNode.layer.tileGrid[jumpIterationNode.i, jumpIterationNode.j] == tileId)
                {
                    context.ApplyCodeCommit(RedrawData.ToolingAll, [], () =>
                    {
                        lastSelectedSectionsSC = jumpIterationNode.sectionAndSC.Item2;
                        lastSelectedSectionsSC.selectedMapSection = jumpIterationNode.sectionAndSC.Item1;
                        lastSelectedSectionsSC.selectedMapSection.SelectLayerOnly(jumpIterationNode.layer);
                        lastSelectedSectionsSC.selectedTileCoords.Replace([new GridCoords(jumpIterationNode.i, jumpIterationNode.j)]);
                    });
                    lastSelectedSectionsSC.canvas.CenterScrollToPos(jumpIterationNode.j * tileset.tileSize, jumpIterationNode.i * tileset.tileSize);
                    return;
                }
            }
        }
        else
        {
            for (int index = jumpIterationNodes.Count - 1; index >= 0; index--)
            {
                var jumpIterationNode = jumpIterationNodes[(index + jumpIterationStartIndex) % jumpIterationNodes.Count];

                if (jumpIterationNode.layer.tileGrid[jumpIterationNode.i, jumpIterationNode.j] == tileId)
                {
                    context.ApplyCodeCommit(RedrawData.ToolingAll, [], () =>
                    {
                        lastSelectedSectionsSC = jumpIterationNode.sectionAndSC.Item2;
                        lastSelectedSectionsSC.selectedMapSection = jumpIterationNode.sectionAndSC.Item1;
                        lastSelectedSectionsSC.selectedMapSection.SelectLayerOnly(jumpIterationNode.layer);
                        lastSelectedSectionsSC.selectedTileCoords.Replace([new GridCoords(jumpIterationNode.i, jumpIterationNode.j)]);
                    });
                    lastSelectedSectionsSC.canvas.CenterScrollToPos(jumpIterationNode.j * tileset.tileSize, jumpIterationNode.i * tileset.tileSize);
                    return;
                }
            }
        }

        Prompt.ShowMessage($"Tile id {tileId} not found in any map section");
    }
}
