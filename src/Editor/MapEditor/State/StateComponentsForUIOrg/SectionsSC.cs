using Editor;
using Shared;
using System.Drawing;
using System.Windows.Input;

namespace MapEditor;

// Represents a list of map or scratch sections and the associated canvas. Due to sheer size of this class, it is split into multiple files
public partial class SectionsSC : StateComponent
{
    public State state;
    public MapCanvas canvas;
    public LayerRenderer layerRenderer => selectedMapSection.layerRenderer;
    public TilesetSC tilesetSC => state.tilesetSC;
    public Tileset tileset => state.tileset;
    public int TS => tileset.tileSize;
    public HotkeyManager hotkeyManager;
    public bool isDirty => mapSections.Any(ms => ms.isDirty);
    public bool isScratch;
    public DirtyFlag defaultDirtyFlag => isScratch ? DirtyFlag.Scratch : DirtyFlag.Map;
    public RedrawTarget redrawTarget => isScratch ? RedrawTarget.Scratch : RedrawTarget.Map;

    public TrackableList<MapSection> mapSections { get => TrListGet<MapSection>(); init => TrListSet(value); }
    public EntranceZoneData GetEntranceZoneData() => new EntranceZoneData(ToModel(), tileset.tileSize);

    public MapSection selectedMapSection { get => TrGet<MapSection>(); set => TrSetAll(value, editorEvent: EditorEvent.SectionChange, onChangeCallback: OnSelectedMapSectionChange); }
    public void OnSelectedMapSectionChange(MapSection oldSection, MapSection newSection)
    {
        selectedTileCoords.Clear();
    }

    public MapSectionLayer? GetSelectedLayer()
    {
        return selectedMapSection.GetSelectedLayer();
    }

    // It is useful to have a quick reference to the other (scratch if map, map if scratch) for things like "tool mirroring" and such
    public SectionsSC? otherSectionsSC;

    public bool magentaBgColor { get => TrGet<bool>(); set => TrSet(value); }
    public bool showUnselectedLayers { get => TrGet<bool>(); set => TrSet(value); }

    public SectionsSC(State state, bool isScratch, CanvasControl canvasControl) : base(state.context)
    {
        this.state = state;
        this.isScratch = isScratch;

        if (!isScratch)
        {
            mapSections = new(state.workspace.mapSections.Select(ms => new MapSection(context, ms)));
            selectedMapSection = mapSections.FirstOrDefault(ms => ms.name == WorkspaceConfig.main.lastMapSection) ?? mapSections[0];
        }
        else
        {
            mapSections = new(state.workspace.scratchSections.Select(ss => new MapSection(context, ss)));
            selectedMapSection = mapSections.FirstOrDefault(ms => ms.name == WorkspaceConfig.main.lastScratchSection) ?? mapSections[0];
        }

        hotkeyManager = new();
        AddCommonHotkeys(hotkeyManager);
        AddEditEntityModeHotkeys(hotkeyManager);
        AddPaintTileModeHotkeys(hotkeyManager);
        AddEditTileDataModeHotkeys(hotkeyManager);
        state.AddScriptHotkeys(hotkeyManager);

        int mw = UISizes.main.mapCanvasWidth;
        int sw = UISizes.main.scratchCanvasWidth;
        int h = UISizes.main.mapCanvasHeight;
        canvas = new(canvasControl, isScratch ? sw : mw, h, tileset.tileSize, this);

        canvas.onZoomChange = (zoom) => selectedMapSection.lastZoom = zoom;
        canvas.onScrollXChange = (scrollX) => selectedMapSection.lastScrollX = scrollX;
        canvas.onScrollYChange = (scrollY) => selectedMapSection.lastScrollY = scrollY;

        // CONFIG BOOKMARK (LOAD)
        if (!isScratch)
        {
            magentaBgColor = WorkspaceConfig.main.mapCanvasMagentaBgColor;
            showUnselectedLayers = WorkspaceConfig.main.showUnselectedMapLayers;
        }
        else
        {
            magentaBgColor = WorkspaceConfig.main.scratchCanvasMagentaBgColor;
        }
    }

    public List<MapSectionModel> ToModel()
    {
        return mapSections.Select(ms => ms.ToModel()).ToList();
    }

    public void Save(MapWorkspace workspace, bool forceSave)
    {
        foreach (MapSection mapSection in mapSections)
        {
            mapSection.Save(workspace, forceSave);
        }
    }

    public void AddCommonHotkeys(HotkeyManager hotkeyManager)
    {
        hotkeyManager.hotkeys.AddRange([
            new HotkeyConfig(Key.Left, HotkeyModifier.Any, () => MoveSelectionCommit(-1, 0)),
            new HotkeyConfig(Key.Right, HotkeyModifier.Any, () => MoveSelectionCommit(1, 0)),
            new HotkeyConfig(Key.Up, HotkeyModifier.Any, () => MoveSelectionCommit(0, -1)),
            new HotkeyConfig(Key.Down, HotkeyModifier.Any, () => MoveSelectionCommit(0, 1)),
            new HotkeyConfig(Key.Escape, UnselectAllTilesCommit),
            new HotkeyConfig(Key.B, state.ShowCenterCameraBounds),
            new HotkeyConfig(Key.N, state.ShowTopLeftCameraBounds),
            new HotkeyConfig(Key.OemCloseBrackets, () => state.JumpToPlaceUsingTileId(false)),
            new HotkeyConfig(Key.OemOpenBrackets, () => state.JumpToPlaceUsingTileId(true)),
        ]);
    }

    public void OnKeyDown(Key key)
    {
        foreach (HotkeyConfig hotkey in hotkeyManager.hotkeys)
        {
            if (hotkey.IsMatch(key, state.selectedMode))
            {
                hotkey.action();
            }
        }
    }

    public void ChangeTool(CanvasTool newTool)
    {
        canvas.tool = newTool;
        if (state.lastSelectedSectionsSC == this)
        {
            canvas.Focus();
        }
        state.mapSectionsSC.OnPaintToolRadioPropertyChangeds();
    }

    public float GetBoxOutlineWidth()
    {
        return 2.0f / zoom;
    }

    public BitmapDrawer? cachedToolLayerDrawer;
    HashSet<int> processedToolLayerCoords = new();

    // Not to be manually called by anything but MapCanvas
    public void DrawToCanvas(Drawer drawer)
    {
        if (state.showGrid) canvas.DrawGrid(drawer);

        if (cachedToolLayerDrawer == null)
        {
            cachedToolLayerDrawer = new BitmapDrawer(canvas.totalWidth, canvas.totalHeight);
        }
        else if (cachedToolLayerDrawer.width != canvas.totalWidth || cachedToolLayerDrawer.height != canvas.totalHeight)
        {
            cachedToolLayerDrawer.Resize(canvas.totalWidth, canvas.totalHeight);
        }

        if (DrawHighlightedTiles(cachedToolLayerDrawer, processedToolLayerCoords))
        {
            drawer.DrawImage(cachedToolLayerDrawer, dx: 0, dy: 0);
        }

        if (!isScratch)
        {
            if (state.showEntities) DrawInstancesAndZones(drawer);
        }

        if (selectedTileCoords.Count > 0)
        {
            /*
            // This optimization attempt isn't much faster (1ms vs 6ms for large rect selections), but leaving for reference
            if (IsSelectionFullRect())
            {
                drawer.DrawRect(GetSelectionRect(), null, Color.Green, GetBoxOutlineWidth());
            }
            */
            foreach (GridCoords tileCoord in selectedTileCoords)
            {
                if (!RectInBounds(tileCoord.GetRect(TS))) continue;
                drawer.DrawRect(tileCoord.GetRect(TS), null, Color.Green, GetBoxOutlineWidth());
            }
        }

        if (state.showTopLeftCameraBounds)
        {
            drawer.DrawRect(GridRect.CreateFromWH(mouseI, mouseJ, 27, 31).GetRect(TS), Color.Magenta, null, 0, 0.5f);
        }
        else if (state.showCameraBounds)
        {
            var rect = GridRect.CreateFromWH(mouseI, mouseJ, 27, 31).GetRect(TS);
            rect = rect.Clone(-128 + 4, -112 + 4);
            drawer.DrawRect(rect, Color.Magenta, null, 0, 0.5f);
        }
    }

    public void Redraw(RedrawFlag redrawFlag)
    {
        processedToolLayerCoords.Clear();
        cachedToolLayerDrawer?.Clear(Color.Transparent);

        if (selectedMapSection?.layers == null) return;

        layerRenderer.Redraw(redrawFlag, this);

        if (layerRenderer.layerContainerDrawer != null)
        {
            canvas.ResizeTotal(layerRenderer.layerContainerDrawer.width, layerRenderer.layerContainerDrawer.height);
        }

        // Change zoom first because when zoomed in, you can actually scroll down/right further
        // Save these as local vars because ChangeZoom actually can clear em out
        int lastScrollX = selectedMapSection.lastScrollX;
        int lastScrollY = selectedMapSection.lastScrollY;
        if (zoom != selectedMapSection.lastZoom)
        {
            canvas.ChangeZoom(selectedMapSection.lastZoom);
        }

        if (scrollX != lastScrollX || scrollY != lastScrollY)
        {
            canvas.ChangeScrollPos(lastScrollX, lastScrollY);
        }

        canvas.InvalidateImage();
    }

    public void DirtyRedraw(Action commitAction)
    {
        context.ApplyCodeCommit(new(RedrawFlag.Tooling, redrawTarget), defaultDirtyFlag, commitAction);
    }

    public void RedrawWithUndo(Action commitAction)
    {
        context.ApplyCodeCommit(new(RedrawFlag.Tooling, redrawTarget), [], commitAction);
    }

    public int GetDefaultTileId()
    {
        return Tile.TransparentTileId;
    }

    public void ChangeLayerIndexCommit(int direction)
    {
        if (Helpers.ControlHeld()) return;
        if (selectedMapSection.layers.Count == 1) return;

        int layerIndex = 0;
        MapSectionLayer? selectedLayer = selectedMapSection.GetSelectedLayer();
        if (selectedLayer != null)
        {
            layerIndex = selectedMapSection.layers.IndexOf(selectedLayer);
        }
        else
        {
            for (int i = selectedMapSection.layers.Count - 1; i >= 0; i--)
            {
                if (selectedMapSection.layers[i].isSelected)
                {
                    layerIndex = i;
                    break;
                }
            }
        }

        layerIndex += direction;
        if (layerIndex < 0 || layerIndex >= selectedMapSection.layers.Count) return;

        context.ApplyCodeCommit(new(RedrawFlag.Container, redrawTarget), [], () =>
        {
            MapSectionLayer layerToSelect = selectedMapSection.layers[layerIndex];
            selectedMapSection.SelectLayerOnly(layerToSelect);
        });
    }

    public void AddLayerCommit()
    {
        if (selectedMapSection.layers.Count > 9)
        {
            Prompt.ShowError("Can't add more than 10 layers");
            return;
        }

        context.ApplyCodeCommit(new(RedrawFlag.Container, redrawTarget), defaultDirtyFlag, () =>
        {
            MapSectionLayer newLayer = selectedMapSection.AddLayer();
            selectedMapSection.SelectLayerOnly(newLayer);
        });
    }

    public void SelectLayerCommit(MapSectionLayer layerToSelect)
    {
        context.ApplyCodeCommit(new(RedrawFlag.Container, redrawTarget), [], () =>
        {
            selectedMapSection.SelectLayerOnly(layerToSelect);
        });
    }

    public void RemoveLayerCommit(int layerIndexToRemove)
    {
        if (selectedMapSection.layers.Count < 2)
        {
            Prompt.ShowError("Can't remove last layer");
            return;
        }

        context.ApplyCodeCommit(new(RedrawFlag.Container, redrawTarget), defaultDirtyFlag, () =>
        {
            selectedMapSection.RemoveLayer(layerIndexToRemove);
        });
    }

    public FolderPath baseFolderPath => isScratch ? state.workspace.scratchSectionFolderPath : state.workspace.mapSectionFolderPath;

    public void AddMapSectionCommit()
    {
        GridRect? selectionGridRect = GetSelectionGridRect();
        int rows = selectionGridRect?.rows > 25 ? selectionGridRect.Value.rows : selectedMapSection.rowCount;
        int cols = selectionGridRect?.cols > 25 ? selectionGridRect.Value.cols : selectedMapSection.colCount;

        NewSectionDialog newSectionDialog = new(
            mapSections.Select(ms => ms.name),
            rows,
            cols,
            isScratch);
        if (newSectionDialog.ShowDialog() == false) return;

        MapSection newMapSection = new(context, newSectionDialog.name, isScratch, newSectionDialog.rows, newSectionDialog.cols);

        // See "Operations that create new files on disk..." pattern for why we are saving immediately and not undo'ing a change that adds file to disk
        mapSections.Add(newMapSection);
        state.workspace.SaveMapSection(newMapSection.ToModel());

        // Only changing the selection is what is added to undo stack
        context.ApplyCodeCommit(new(RedrawFlag.All, redrawTarget), [], () =>
        {
            selectedMapSection = newMapSection;
        });
    }

    public void ImportMapSection()
    {
        ImportSectionDialog importSectionDialog = new();
        if (importSectionDialog.ShowDialog() == false) return;
        string path = importSectionDialog.SelectedPath;
        if (path.Unset()) return;

        string importImageFileName = FilePath.New(path).fileNameNoExt;

        if (mapSections.Any(ms => ms.name == importImageFileName))
        {
            Prompt.ShowError($"Map section with name {importImageFileName} already exists");
            return;
        }

        ImageImporter mii = new ImageImporter().Import(new FilePath(path), tileset);
        MapSection newMapSection = new(context, importImageFileName, isScratch, mii.importGrid);

        mapSections.Add(newMapSection);
        newMapSection.Save(state.workspace, true);
        tilesetSC.Save(state.workspace, true);

        if (mii.numberOfNewTilesAdded > 0)
        {
            Prompt.ShowMessage($"Imported {mii.numberOfNewTilesAdded} new tiles from image.");
        }

        undoManager?.Nuke();
    }

    public void SetSelectionText(int mouseX, int mouseY)
    {
        state.mouseClickedText = $"{mouseX.ToString("0")},{mouseY.ToString("0")}";
        state.OnPropertyChanged(nameof(state.mouseClickedText));
        if (!selectedMapSection.firstLayer.tileGrid.InRange(mouseY / TS, mouseX / TS)) return;

        int tileId = GetTopTileId(mouseY / TS, mouseX / TS);
        Tile tile = state.tileset.GetTileById(tileId);
        Color[,] pixels = tileset.GetColorsFromTileHash(tile.hash, false);
        state.colorClickedText = Helpers.ColorToHexString(pixels[mouseY % TS, mouseX % TS]);
        state.OnPropertyChanged(nameof(state.colorClickedText));
    }

    public void ResizeMapSectionCommit(int rows, int cols, bool fromTopLeft)
    {
        context.ApplyCodeCommit(new(RedrawFlag.All, redrawTarget), defaultDirtyFlag, () =>
        {
            selectedMapSection.Resize(rows, cols, TS, fromTopLeft);
            selectedMapSection.QueueOnPropertyChanged(nameof(selectedMapSection.displaySize));
        });
    }
}
