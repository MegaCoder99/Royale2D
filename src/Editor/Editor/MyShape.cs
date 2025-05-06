using Shared;

namespace Editor;

public class MyShape
{
    public List<MyPoint> points;

    public MyShape(List<MyPoint> points)
    {
        this.points = points;
    }

    public MyShape Clone(int x, int y)
    {
        var points = new List<MyPoint>();
        for (var i = 0; i < this.points.Count; i++)
        {
            var point = this.points[i];
            points.Add(new MyPoint(point.x + x, point.y + y));
        }
        return new MyShape(points);
    }
}
