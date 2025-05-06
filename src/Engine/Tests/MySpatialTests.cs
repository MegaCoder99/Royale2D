using Royale2D;

namespace Tests;

[TestClass]
public class MySpatialTests
{
    [TestMethod]
    public void TestRectsIntersectWithOverlappingRects()
    {
        var rect1 = new IntRect(0, 0, 10, 10);
        var rect2 = new IntRect(5, 5, 15, 15);
        bool result = MySpatial.RectsIntersect(rect1, rect2);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestRectsIntersectWithNonOverlappingRects()
    {
        var rect1 = new IntRect(0, 0, 10, 10);
        var rect2 = new IntRect(20, 20, 30, 30);
        bool result = MySpatial.RectsIntersect(rect1, rect2);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestRectIntersectsIrtWithOverlapping()
    {
        var rect = new IntRect(0, 0, 10, 10);
        var irt = new IntIrt(new IntPoint(5, 5), 5, IrtDir.BottomRight);
        bool result = MySpatial.RectIntersectsIrt(rect, irt);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestRectIntersectsIrtWithOverlapping2()
    {
        var rect = new IntRect(0, 0, 6, 6);
        var irt = new IntIrt(new IntPoint(10, 10), 10, IrtDir.BottomRight);
        bool result = MySpatial.RectIntersectsIrt(rect, irt);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestRectIntersectsIrtWithNoOverlap()
    {
        var rect = new IntRect(0, 0, 10, 10);
        var irt = new IntIrt(new IntPoint(20, 20), 5, IrtDir.BottomRight);
        bool result = MySpatial.RectIntersectsIrt(rect, irt);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TestRectIntersectsIrtWithNoOverlap2()
    {
        var rect = new IntRect(0, 0, 1, 1);
        var irt = new IntIrt(new IntPoint(10, 10), 10, IrtDir.BottomRight);
        bool result = MySpatial.RectIntersectsIrt(rect, irt);
        Assert.IsFalse(result);
    }
}