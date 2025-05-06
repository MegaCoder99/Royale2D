
namespace Shared;

public struct MyRect
{
    public int x1 { get; set; }
    public int y1 { get; set; }
    public int x2 { get; set; }
    public int y2 { get; set; }

    public int w => x2 - x1;
    public int h => y2 - y1;
    public int hw => w / 2;
    public int hh => h / 2;
    public int hw2 => MyMath.DivideRoundUp(w, 2);
    public int hh2 => MyMath.DivideRoundUp(h, 2);

    public int area => w * h;

    public MyRect()
    {
    }

    public MyRect(int x1, int y1, int x2, int y2)
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    public static MyRect CreateWH(int x, int y, int w, int h)
    {
        return new MyRect(x, y, x + w, y + h);
    }

    public static MyRect CreateFromStringKey(string key)
    {
        var pieces = key.Split('_');
        return new MyRect(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]));
    }

    public MyRect AddXY(int x, int y)
    {
        return CreateWH(x1 + x, y1 + y, w, h);
    }

    public bool Overlaps(MyRect other)
    {
        // If one rectangle is on left side of other
        if (x1 > other.x2 || other.x1 > x2)
            return false;
        // If one rectangle is above other
        if (y1 > other.y2 || other.y1 > y2)
            return false;
        return true;
    }

    public bool Contains(MyPoint point)
    {
        return point.x >= x1 && point.x <= x2 && point.y >= y1 && point.y <= y2;
    }

    public bool Contains(float x, float y)
    {
        return x >= x1 && x <= x2 && y >= y1 && y <= y2;
    }

    public bool Equals(MyRect other)
    {
        return x1 == other.x1 && x2 == other.x2 && y1 == other.y1 && y2 == other.y2;
    }

    public MyRect Clone(int x, int y)
    {
        return new MyRect(x1 + x, y1 + y, x2 + x, y2 + y);
    }

    public override string ToString()
    {
        return x1 + "_" + y1 + "_" + x2 + "_" + y2;
    }

    public MyPoint Center()
    {
        return new MyPoint((x1 + x2) / 2, (y1 + y2) / 2);
    }

    public bool EqualTo(MyRect rect)
    {
        return x1 == rect.x1 && y1 == rect.y1 && x2 == rect.x2 && y2 == rect.y2;
    }

    public MyRect AddSize(int delta)
    {
        return new MyRect(x1 - delta, y1 - delta, x2 + delta, y2 + delta);
    }
}