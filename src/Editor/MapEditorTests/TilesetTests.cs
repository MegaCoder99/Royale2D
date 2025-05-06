using MapEditor;
using Shared;
using System.Drawing;

namespace MapEditorTests;

[TestClass]
public class TilesetTests
{
    private const string transparentTileHash = "////AA==|0000000000000000000000000000000000000000000000000000000000000000";

    private Tileset GetMockTileset(string? overrideHash = null)
    {
        return new Tileset(
        [
            new Tile
            {
                id = 0,
                hash = overrideHash ?? transparentTileHash,
            }
        ]);
    }

    [TestMethod]
    public void Test_GetTileSizeFromHash()
    {
        Tileset tileset = GetMockTileset();

        Assert.AreEqual(8, tileset.GetTileSizeFromHash(transparentTileHash));
    }

    [TestMethod]
    public void Test_GetCharIndex()
    {
        Tileset tileset = GetMockTileset();

        Assert.AreEqual(0, tileset.GetCharIndex('0'));
        Assert.AreEqual(1, tileset.GetCharIndex('1'));
        Assert.AreEqual(9, tileset.GetCharIndex('9'));
        Assert.AreEqual(10, tileset.GetCharIndex('a'));
        Assert.AreEqual(11, tileset.GetCharIndex('b'));
        Assert.AreEqual(34, tileset.GetCharIndex('y'));
        Assert.AreEqual(35, tileset.GetCharIndex('z'));
        Assert.AreEqual(36, tileset.GetCharIndex('A'));
        Assert.AreEqual(37, tileset.GetCharIndex('B'));
        Assert.AreEqual(60, tileset.GetCharIndex('Y'));
        Assert.AreEqual(61, tileset.GetCharIndex('Z'));
    }

    [TestMethod]
    public void Test_GetSingleCharIndex()
    {
        Tileset tileset = GetMockTileset();

        Assert.AreEqual("0", tileset.GetSingleCharIndex(0));
        Assert.AreEqual("1", tileset.GetSingleCharIndex(1));
        Assert.AreEqual("9", tileset.GetSingleCharIndex(9));
        Assert.AreEqual("a", tileset.GetSingleCharIndex(10));
        Assert.AreEqual("b", tileset.GetSingleCharIndex(11));
        Assert.AreEqual("y", tileset.GetSingleCharIndex(34));
        Assert.AreEqual("z", tileset.GetSingleCharIndex(35));
        Assert.AreEqual("A", tileset.GetSingleCharIndex(36));
        Assert.AreEqual("B", tileset.GetSingleCharIndex(37));
        Assert.AreEqual("Y", tileset.GetSingleCharIndex(60));
        Assert.AreEqual("Z", tileset.GetSingleCharIndex(61));
    }

    [TestMethod]
    public void Test_GetColorPoolFromTileHashPrefix()
    {
        Tileset tileset = GetMockTileset();
        List<Color> colorPool = tileset.GetColorPoolFromTileHashPrefix("qJho/w== gHAw/w==");

        Assert.AreEqual(Color.FromArgb(255, 168, 152, 104), colorPool[0]);
        Assert.AreEqual(Color.FromArgb(255, 128, 112, 48), colorPool[1]);
    }

    [TestMethod]
    public void Test_GetTileHashFromColors()
    {
        Color[,] colors = new Color[2,2];
        colors[0,0] = Color.FromArgb(255, 0, 0, 0);
        colors[0,1] = Color.FromArgb(255, 255, 0, 0);
        colors[1,0] = Color.FromArgb(255, 0, 255, 0);
        colors[1,1] = Color.FromArgb(255, 0, 0, 255);

        var tileset = GetMockTileset("////AA==|0000");
        var hash = tileset.GetTileHashFromColors(colors);

        Assert.AreEqual("AAAA/w== /wAA/w== AP8A/w== AAD//w==|0123", hash);
    }

    public void Test_GetTileColorsFromHash()
    {
        Tileset tileset = GetMockTileset("////AA==|0000");

        Color[,] colors = tileset.GetColorsFromTileHash("AAAA/w== /wAA/w== AP8A/w== AAD//w==|0123", false);

        Assert.AreEqual(Color.FromArgb(255, 0, 0, 0), colors[0, 0]);
        Assert.AreEqual(Color.FromArgb(255, 255, 0, 0), colors[0, 1]);
        Assert.AreEqual(Color.FromArgb(255, 0, 255, 0), colors[1, 0]);
        Assert.AreEqual(Color.FromArgb(255, 0, 0, 255), colors[1, 1]);
    }
}