namespace Shared;

public record TileClumpModel(
    string name,
    int[,] tileIds,
    string transformTileClumpNameCsv,
    string tags,
    string properties,
    List<TileClumpSubsectionModel> subsections);

public record TileClumpSubsectionModel(
    string name,
    List<GridCoords> cells);