using Editor;
using Shared;

namespace MapEditor;

public record TileInstance(int i, int j, int tileId);

public class MapSectionLayer : StateComponentWithModel<MapSectionLayerModel>
{
    #region model section
    // For undo'ability and efficient redraw, do not modify cells directly, use ChangeTileGrid instead
    public int[,] tileGrid { get; set; }

    public MapSectionLayerModel ToModel()
    {
        return new MapSectionLayerModel(tileGrid);
    }
    #endregion

    // Don't reference drawer directly to avoid null ref, use GetDrawer instead. This is a "lazy" with a parameter
    public BitmapDrawer? drawer;
    public BitmapDrawer GetDrawer(Tileset tileset) => drawer ??= tileset.CreateDrawerAndDrawTileGrid(tileGrid);

    public List<TileInstance> pendingTileRedrawDiffs = [];
    public string displayName { get; set; } = "Layer";
    public bool isSelected { get => TrGetD(true); set => TrSetV(value, ValidateIsSelected); }

    public bool ValidateIsSelected(bool newValue)
    {
        if (context.editorState is State state)
        {
            if (state.mapSectionsSC?.selectedMapSection?.GetSelectedLayer() == this || state.scratchSectionsSC?.selectedMapSection?.GetSelectedLayer() == this)
            {
                // IMPROVE for usability, could we remove this check? need to analyze the situation and all places that could throw as a result, and guard
                Prompt.ShowError("Must have at least one layer selected at all times.");
                return false;
            }
        }
        return true;
    }

    public MapSectionLayer(EditorContext context, MapSectionLayerModel model) : base(context, model)
    {
        tileGrid = model.tileGrid;
    }

    public MapSectionLayer(EditorContext context, int[,] tileGrid) : 
        this (context, new MapSectionLayerModel(tileGrid))
    {
        this.tileGrid = tileGrid;
    }

    public MapSectionLayer(EditorContext context, int rows, int cols) : 
        this(context, new MapSectionLayerModel(Helpers.Create2DArray(rows, cols, Tile.TransparentTileId)))
    {
    }

    public void UpdateDisplayName(int i)
    {
        displayName = i.ToString();
        OnPropertyChanged(nameof(displayName));
    }

    public bool ChangeTileGrid(List<TileInstance> tileInstances, bool refreshTileUI = false)
    {
        // Deep clone to be safe
        tileInstances = new List<TileInstance>(tileInstances);

        // Remove any invalid tile instances or ones with no changes
        for (int i = tileInstances.Count - 1; i >= 0; i--)
        {
            TileInstance tileInstance = tileInstances[i];
            if (tileInstance.i < 0 || tileInstance.j < 0 || tileInstance.i >= tileGrid.GetLength(0) || tileInstance.j >= tileGrid.GetLength(1) || 
                tileInstance.tileId == tileGrid[tileInstance.i, tileInstance.j])
            {
                tileInstances.RemoveAt(i);
            }
        }

        if (tileInstances.Count == 0) return false;

        List<TileInstance> oldTileInstances = [];
        foreach (TileInstance ti in tileInstances)
        {
            oldTileInstances.Add(new TileInstance(ti.i, ti.j, tileGrid[ti.i, ti.j]));
        }

        var undoAction = () =>
        {
            foreach (TileInstance tileInstance in oldTileInstances)
            {
                tileGrid[tileInstance.i, tileInstance.j] = tileInstance.tileId;
            }
            pendingTileRedrawDiffs.AddRange(oldTileInstances);
        };

        var redoAction = () =>
        {
            foreach (TileInstance tileInstance in tileInstances)
            {
                tileGrid[tileInstance.i, tileInstance.j] = tileInstance.tileId;
            }
            pendingTileRedrawDiffs.AddRange(tileInstances);
        };

        redoAction.Invoke();

        if (refreshTileUI)
        {
            context.FireEvent(EditorEvent.LayerTileGridChange, this);
        }

        undoManager?.AddUndoNode(new UndoNode(undoAction, redoAction, null));

        return true;
    }

    public bool RedrawTileBitmapDiff(SectionsSC sectionsSC)
    {
        int TS = sectionsSC.TS;
        if (pendingTileRedrawDiffs.Count == 0)
        {
            return false;
        }
        else
        {
            foreach (TileInstance changes in pendingTileRedrawDiffs)
            {
                Tile tile = sectionsSC.tileset.GetTileById(changes.tileId);
                Drawer tileToDraw = sectionsSC.tileset.GetDrawerFromTileHash(tile.hash);
                GetDrawer(sectionsSC.tileset).DrawImage(tileToDraw, changes.j * TS, changes.i * TS, overwritePixels: true);
            }

            pendingTileRedrawDiffs.Clear();
            return true;
        }
    }

    public Tile GetTile(Tileset tileset, int i, int j)
    {
        int tileId = tileGrid[i, j];
        return tileset.GetTileById(tileId);
    }
}