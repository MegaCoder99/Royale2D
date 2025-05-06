namespace Shared;

public struct MyPoint
{
    public int x { get; set; }
    public int y { get; set; }

    public MyPoint()
    {
    }

    public MyPoint(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public MyPoint AddXY(int x, int y)
    {
        var point = new MyPoint(this.x + x, this.y + y);
        return point;
    }

    public static MyPoint operator +(MyPoint a, MyPoint b)
    {
        return new MyPoint(a.x + b.x, a.y + b.y);
    }

    public static MyPoint operator -(MyPoint a, MyPoint b)
    {
        return new MyPoint(a.x - b.x, a.y - b.y);
    }
}