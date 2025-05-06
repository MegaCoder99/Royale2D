using Royale2D;

namespace Tests;

[TestClass]
public class GuiTests
{
    static string guiXml =
@"
<div id=""root"" width=""256"" height=""224"">
  <vdiv id=""inventory"" spacing=""2"">
    <hdiv>
      <image id=""item1"" sprite=""hud_item_box"" />
    </hdiv>
    <hdiv>
      <image id=""item2"" sprite=""hud_item_box"" />
    </hdiv>
  </vdiv>
</div>
";

    [TestMethod]
    public void TestGuiParser()
    {
        Assets.LoadImages();
        Assets.LoadSprites();
        // Test guiXml parsing
        var gui = Gui.FromString(guiXml);
        var pos = gui.GetNodeById("item2").GetPos();
        Assert.AreEqual(0, pos.x);
        Assert.AreEqual(20, pos.y);
    }
}
