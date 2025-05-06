namespace Royale2D
{
    public struct Rect
    {
        public float x1;
        public float y1;
        public float x2;
        public float y2;
        public float w => x2 - x1;
        public float h => y2 - y1;
        public float area => w * h;
        public float midX => (x1 + x2) * 0.5f;

        public Rect(float x1, float y1, float x2, float y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public static Rect Create(Point topLeftPoint, Point botRightPoint)
        {
            return new Rect(topLeftPoint.x, topLeftPoint.y, botRightPoint.x, botRightPoint.y);
        }

        public static Rect CreateWH(float x, float y, float w, float h)
        {
            return new Rect(x, y, x + w, y + h);
        }

        public static Rect CreateFromStringKey(string key)
        {
            var pieces = key.Split('_');
            return new Rect(float.Parse(pieces[0]), float.Parse(pieces[1]), float.Parse(pieces[2]), float.Parse(pieces[3]));
        }

        public bool Equals(Rect other)
        {
            return x1 == other.x1 && x2 == other.x2 && y1 == other.y1 && y2 == other.y2;
        }

        public Rect Clone(float x, float y)
        {
            return new Rect(x1 + x, y1 + y, x2 + x, y2 + y);
        }

        public override string ToString()
        {
            return x1 + "," + y1 + "," + x2 + "," + y2;
        }

        /*
        public Shape GetShape()
        {
            return new Shape(new List<Point>() { topLeftPoint, new Point(x2, y1), botRightPoint, new Point(x1, y2) });
        }

        public List<Point> GetPoints()
        {
            return new List<Point>()
            {
                new Point(topLeftPoint.x, topLeftPoint.y),
                new Point(botRightPoint.x, topLeftPoint.y),
                new Point(botRightPoint.x, botRightPoint.y),
                new Point(topLeftPoint.x, botRightPoint.y),
            };
        }
        */

        public Point GetCenter()
        {
            return new Point(x1 + (w / 2f), y1 + (h / 2f));
        }

        public bool Overlaps(Rect other, bool countEdges)
        {
            if (countEdges)
            {
                if (x1 > other.x2 || other.x1 > x2 || y1 > other.y2 || other.y1 > y2) return false;
            }
            else
            {
                if (x1 >= other.x2 || other.x1 >= x2 || y1 >= other.y2 || other.y1 >= y2) return false;
            }
            return true;
        }

        public float? GetOverlapX(Rect other)
        {
            float ax1 = x1;
            float ax2 = x2;
            float bx1 = other.x1;
            float bx2 = other.x2;

            float start = Math.Max(ax1, bx1); // Start of the potential overlap
            float end = Math.Min(ax2, bx2); // End of the potential overlap

            // If the end of the overlap is before the start, there's no overlap
            if (end < start) return null;

            // If the end equals start, it means they are touching at the edge without overlapping
            if (end == start) return null;

            int sign = 1;
            if (GetCenter().x < other.GetCenter().x) sign = -1;

            // Otherwise, the overlap distance is the difference between end and start
            return sign * (end - start);
        }

        public float? GetOverlapY(Rect other)
        {
            float ay1 = y1;
            float ay2 = y2;
            float by1 = other.y1;
            float by2 = other.y2;

            float start = Math.Max(ay1, by1); // Start of the potential overlap
            float end = Math.Min(ay2, by2); // End of the potential overlap

            // If the end of the overlap is before the start, there's no overlap
            if (end < start) return null;

            // If the end equals start, it means they are touching at the edge without overlapping
            if (end == start) return null;

            int sign = 1;
            if (GetCenter().y < other.GetCenter().y) sign = -1;

            // Otherwise, the overlap distance is the difference between end and start
            return sign * (end - start);
        }
    }
}
