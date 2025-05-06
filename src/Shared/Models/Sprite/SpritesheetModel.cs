namespace Shared;

public record SpritesheetModel(
    string name,    // Includes sub-path in folder containing all spritesheets if applicable, as well as extension, i.e. "foo/bar/spritesheet.png"
    FilePath filePath   // Full file path on disk
);
