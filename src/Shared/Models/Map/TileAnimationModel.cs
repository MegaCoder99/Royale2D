namespace Shared;

public record TileAnimationModel(
    int id,
    List<int> tileIds,
    int? duration   // If not set, your game engine picks a default
);