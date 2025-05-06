namespace Shared;

public record MapSectionModel(
    string name,        // Not saved to disk, but in the model class to simplify runtime code
    bool? isScratch,    // Not saved to disk, but in the model class to simplify runtime code
    List<MapSectionLayerModel> layers,
    List<InstanceModel> instances,
    List<ZoneModel> zones,
    string defaultMusicName,
    string defaultEntranceDir,
    string defaultMaskColor,
    int startLayer  // Currently not settable in editor, have to manually edit json for this
)
{
    public int rowCount => layers[0].tileGrid.GetLength(0);
    public int colCount => layers[0].tileGrid.GetLength(1);

    // Don't expose this because it results in possibility of someone setting [] as the layers which is invalid, there must always be at least one layer
    private static MapSectionModel New(string name, bool isScratch, List<MapSectionLayerModel> layers)
    {
        return new(name, isScratch, layers, [], [], "", "", "", 0);
    }

    public static MapSectionModel New(string name, bool isScratch, int[,] initialTileGrid)
    {
        return New(name, isScratch, [new MapSectionLayerModel(initialTileGrid)]);
    }

    public static MapSectionModel New(string name, bool isScratch, int width, int height)
    {
        return New(name, isScratch, [new MapSectionLayerModel(width, height)]);
    }

    public static MapSectionModel New(string name, bool isScratch, MapSectionLayerModel initialLayer)
    {
        return New(name, isScratch, [initialLayer]);
    }
}