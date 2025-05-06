namespace Shared;

public class MyMath
{
    public static int Snap(int a, int b)
    {
        return (a / b) * b;
    }

    public static int Floor(float val)
    {
        return (int)Math.Floor(val);
    }

    public static int Ceil(float val)
    {
        return (int)Math.Ceiling(val);
    }

    public static int Round(float val)
    {
        return (int)Math.Round(val);
    }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static int ClampMin0(int value)
    {
        return Clamp(value, 0, int.MaxValue);
    }

    public static int ClampMax(int value, int max)
    {
        return Clamp(value, int.MinValue, max);
    }

    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static float ClampMin0(float value)
    {
        return Clamp(value, 0, float.MaxValue);
    }

    public static float ClampMax(float value, float max)
    {
        return Clamp(value, float.MinValue, max);
    }

    public static float SinD(float degrees)
    {
        var radians = degrees * MathF.PI / 180f;
        return MathF.Sin(radians);
    }

    public static float CosD(float degrees)
    {
        var radians = degrees * MathF.PI / 180f;
        return MathF.Cos(radians);
    }

    public static int ArcTanD(float value)
    {
        // When making this netcode safe, consider large values and approximating them to PI/2
        float radians = MathF.Atan(value);
        return (int)(radians * 180 / Math.PI);
    }

    public static int DivideRoundUp(int dividend, int divisor)
    {
        if (dividend % divisor != 0)
        {
            return (dividend / divisor) + 1;
        }
        return dividend / divisor;
    }

    public static float ClampMin(float val, float min)
    {
        if (val < min) return min;
        return val;
    }

    public static int Floor(double val)
    {
        return (int)Math.Floor(val);
    }

    public static int Ceil(double val)
    {
        return (int)Math.Ceiling(val);
    }

    public static int Round(double val)
    {
        return (int)Math.Round(val);
    }

    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static double ClampMin0(double value)
    {
        return Clamp(value, 0, double.MaxValue);
    }

    public static double ClampMax(double value, double max)
    {
        return Clamp(value, double.MinValue, max);
    }

    public static double ClampMin(double val, double min)
    {
        if (val < min) return min;
        return val;
    }
}
