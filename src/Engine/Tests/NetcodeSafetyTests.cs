using Royale2D;

namespace Tests;

[TestClass]
public class NetcodeSafetyTests
{
    public NetcodeSafetyTests()
    {
        LookupTables.Init();
    }

    [TestMethod]
    public void TestSqrt()
    {
        for (float i = 0; i < 10000; i += 0.01f)
        {
            Fd fdVal = Fd.New(i);
            Fd fdResult = NetcodeSafeMath.Sqrt(fdVal.longFd);
            float fdResultFloat = fdResult.floatVal;

            float floatVal = i;
            float floatResult = (float)Math.Sqrt(floatVal);

            if (Math.Abs(fdResultFloat - floatResult) > 0.1)
            {
                Assert.Fail($"Test failed for {i}. Expected {floatResult}, got {fdResultFloat}");
                return;
            }
        }
    }

    [TestMethod]
    public void TestATan()
    {
        for (int x = -1000; x < 1000; x++)
        {
            for (int y = -1000; y < 1000; y++)
            {
                Fd fdX = Fd.New(x);
                Fd fdY = Fd.New(y);
                Fd fdResult = NetcodeSafeMath.ArcTanD(fdY, fdX);
                float fdResultFloat = fdResult.floatVal;

                float floatX = x;
                float floatY = y;
                float floatResult = (float)Math.Atan2(floatY, floatX) * (float)(180 / Math.PI);
                while (floatResult < 0) floatResult += 360;
                while (floatResult >= 360) floatResult -= 360;

                if (Math.Abs(fdResultFloat - floatResult) > 2)
                {
                    Assert.Fail($"Test failed for {x}, {y}. Expected {floatResult}, got {fdResultFloat}");
                    return;
                }
            }
        }
    }
}