using SpriteEditor;
using Shared;

namespace SpriteEditorTests;

[TestClass]
public class SpritesheetTests
{
    [TestMethod]
    public void Test_GetPixelClumpRect()
    {
        var spritesheet = new Spritesheet("mock_spritesheet", new FilePath("MockImages/shield.png"));

        MyRect? nullResult = spritesheet.GetPixelClumpRect(0, 0);
        MyRect? nonNullResult = spritesheet.GetPixelClumpRect(19, 14);

        Assert.IsNull(nullResult);
        Assert.IsNotNull(nonNullResult);
        Assert.AreEqual(16, nonNullResult.Value.x1);
        Assert.AreEqual(8, nonNullResult.Value.y1);
        Assert.AreEqual(23, nonNullResult.Value.x2);
        Assert.AreEqual(21, nonNullResult.Value.y2);
    }
}