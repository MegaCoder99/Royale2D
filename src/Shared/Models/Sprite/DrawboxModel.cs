namespace Shared;

public record DrawboxModel(
    MyPoint pos,    // Position of the drawbox relative to the frame's origin
    string spritesheetName,
    MyRect rect,    // The area of the spritesheet to draw
    string tags,
    long zIndex
)
{
    public static DrawboxModel New(string spritesheetName, MyRect rect)
    {
        return new DrawboxModel(new(), spritesheetName, rect, "", 0);
    }
}
