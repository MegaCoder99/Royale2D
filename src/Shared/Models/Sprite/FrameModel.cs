namespace Shared;

public record FrameModel(
    MyRect rect,
    int duration,
    MyPoint offset,
    List<HitboxModel> hitboxes,
    List<DrawboxModel> drawboxes,
    List<POIModel> POIs,
    string tags,
    // Only used in final exported sprites output if packaged. Editor doesn't use this because it has the sprite use a common one for all frames.
    // Game engine will need to respect this one if it exists, and ignore the top-level sprite one if so
    string spritesheetName
)
{
    public static FrameModel New(MyRect rect, int duration, MyPoint offset)
    {
        return new FrameModel(rect, duration, offset, [], [], [], "", "");
    }
}
