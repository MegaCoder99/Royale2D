using Editor;
using Shared;

namespace MapEditor;

public class MapSection : StateComponentWithModel<MapSectionModel>
{
    #region model section

    // Don't modify layers directly, use helper functions AddLayer, RemoveLayer, etc
    public TrackableList<MapSectionLayer> layers { get => TrListGet(model.layers.Select(l => new MapSectionLayer(context, l))); init => TrListSet(value); }
    public TrackableList<Instance> instances { get => TrListGet(model.instances.Select(i => Instance.New(context, i))); init => TrListSet(value); }
    public TrackableList<Zone> zones { get => TrListGet(model.zones.Select(z => new Zone(context, z))); init => TrListSet(value); }
    public string name { get => TrGetD(model.name); set => TrSet(value); }
    public bool isScratch { get => TrGetD(model.isScratch ?? false); set => TrSet(value); }
    public string defaultMusicName { get => TrGetD(model.defaultMusicName); set => TrSet(value); }
    public string defaultEntranceDir { get => TrGetD(model.defaultEntranceDir); set => TrSet(value); }
    public string defaultMaskColor { get => TrGetD(model.defaultMaskColor); set => TrSet(value); }
    public int startLayer { get => TrGetD(model.startLayer); set => TrSet(value); }

    public MapSectionModel ToModel()
    {
        return new MapSectionModel(
            name,
            isScratch,
            layers.SelectList(l => l.ToModel()),
            instances.SelectList(i => i.ToModel()),
            zones.SelectList(z => z.ToModel()), 
            defaultMusicName, 
            defaultEntranceDir, 
            defaultMaskColor,
            startLayer
        );
    }
    #endregion

    public MapSectionLayer firstLayer => layers[0];
    // Only returns non-null if a single layer is selected
    public MapSectionLayer? GetSelectedLayer()
    {
        if (layers.Count(l => l.isSelected) > 1) return null;
        return layers.FirstOrDefault(l => l.isSelected);
    }
    public void SelectLayerOnly(MapSectionLayer layerToSelect)
    {
        layerToSelect.isSelected = true;
        foreach (MapSectionLayer layer in layers)
        {
            if (layer != layerToSelect) layer.isSelected = false;
        }
    }

    public Instance? selectedInstance { get => TrGet<Instance?>(); set => TrSetC(value, OnSelectedInstanceChange); }
    public void OnSelectedInstanceChange(Instance? oldInstance, Instance? newInstance)
    {
        QueueOnPropertyChanged(nameof(showSelectedInstanceProperties));
        if (newInstance != null)
        {
            selectedZone = null;
        }
    }
    public bool showSelectedInstanceProperties => selectedInstance != null;

    public Zone? selectedZone { get => TrGet<Zone?>(); set => TrSetC(value, OnSelectedZoneChange); }
    public void OnSelectedZoneChange(Zone? oldZone, Zone? newZone)
    {
        QueueOnPropertyChanged(nameof(showSelectedZoneProperties));
        if (newZone != null)
        {
            selectedInstance = null;
        }
    }
    public bool showSelectedZoneProperties => selectedZone != null;

    public bool isDirty { get => TrGet<bool>(); set => TrSet(value, [nameof(displayName)]); }
    public override string ToString() => name;
    public string displayName => name + (isDirty ? "*" : "");
    public string displaySize => $"{rowCount} rows, {colCount} cols";
    public int lastScrollX { get; set; }
    public int lastScrollY { get; set; }
    public int lastZoom { get; set; } = 1;

    public LayerRenderer layerRenderer = new LayerRenderer();

    public int rowCount => layers[0].tileGrid.GetLength(0);
    public int colCount => layers[0].tileGrid.GetLength(1);
    
    public MapSection(EditorContext context, MapSectionModel model) : base(context, model)
    {
        UpdateLayerDisplayNames();
    }

    public MapSection(EditorContext context, string name, bool isScratch, int width, int height)
        : this(context, MapSectionModel.New(name, isScratch, width, height))
    {
    }

    public MapSection(EditorContext context, string name, bool isScratch, int[,] initialTileGrid)
        : this(context, MapSectionModel.New(name, isScratch, initialTileGrid))
    {
    }

    public void Save(MapWorkspace workspace, bool forceSave)
    {
        if (isDirty || forceSave)
        {
            workspace.SaveMapSection(ToModel());
            isDirty = false;
        }
    }

    public void UpdateLayerDisplayNames()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].UpdateDisplayName(i);
        }
    }

    public MapSectionLayer AddLayer()
    {
        var newTileGrid = Helpers.Create2DArray(rowCount, colCount, Tile.TransparentTileId);
        MapSectionLayer newLayer = new(context, newTileGrid);
        layers.Add(newLayer);

        QueueGenericAction(UpdateLayerDisplayNames);

        return newLayer;
    }

    public void Resize(int newRows, int newCols, int tileSize, bool fromTopLeft)
    {
        int resizeOriginRow = fromTopLeft ? -1 : 1;
        int resizeOriginCol = fromTopLeft ? -1 : 1;
        bool movedEntities = false;
        foreach (MapSectionLayer layer in layers)
        {
            int[,] oldTileGrid = layer.tileGrid;
            (int rowShift, int colShift) = oldTileGrid.GetResizeShift(newRows, newCols, resizeOriginRow, resizeOriginCol);

            // Only move entities once because right now, entities aren't stored on a per-layer basis
            if (!movedEntities)
            {
                if (!MoveEntitiesFromResize(rowShift, colShift, tileSize))
                {
                    return;
                }
                movedEntities = true;
            }

            var undoAction = () =>
            {
                layer.tileGrid = oldTileGrid;
            };

            var redoAction = () =>
            {
                layer.tileGrid = oldTileGrid.ResizeGrid(newRows, newCols, resizeOriginRow, resizeOriginCol);
            };

            redoAction.Invoke();

            undoManager?.AddUndoNode(undoAction, redoAction, null);
        }

        QueueOnPropertyChanged(nameof(displaySize));
    }

    // IMPROVE return false if entities get out-of-bounds?
    public bool MoveEntitiesFromResize(int rowShift, int colShift, int tileSize)
    {
        // Actual move
        foreach (Instance instance in instances)
        {
            instance.pos.y += rowShift * tileSize;
            instance.pos.x += colShift * tileSize;
        }
        foreach (Zone zone in zones)
        {
            if (zone.gridRect != null)
            {
                zone.gridRect.IncIJ(rowShift, colShift);
            }
            else if (zone.rect != null)
            {
                zone.rect.IncXY(colShift * tileSize, rowShift * tileSize);
            }
        }

        return true;
    }

    public void RemoveLayer(int index)
    {
        layers.RemoveAt(index);
        if (!layers.Any(l => l.isSelected))
        {
            layers[MyMath.ClampMin0(index - 1)].isSelected = true;
        }
        QueueGenericAction(UpdateLayerDisplayNames);
    }
}