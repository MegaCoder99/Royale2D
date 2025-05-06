namespace Shared;

public record SpriteModel(
    string name,            // Not saved to disk, but in the model class to simplify runtime code
    string spritesheetName, // Will not be set in engine export if sprite is packaged during export. Instead it would use the one in FrameModel
    List<FrameModel> frames,
    List<HitboxModel> hitboxes,
    int loopStartFrame,
    string alignment,
    string wrapMode
)
{
    public static SpriteModel New(string name, string spritesheetName) =>
        new SpriteModel(name, spritesheetName, [], [], 0, Alignment.Center, WrapMode.Loop);
}
