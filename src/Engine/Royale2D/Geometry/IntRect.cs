namespace Royale2D
{
    public class IntRect : IntShape
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public int w => x2 - x1;
        public int h => y2 - y1;
        public int area => w * h;

        public IntRect(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;

            _points = new List<IntPoint>()
            {
                new IntPoint(x1, y1),
                new IntPoint(x2, y1),
                new IntPoint(x2, y2),
                new IntPoint(x1, y2),
            };
        }

        public static IntRect CreateWH(int x, int y, int w, int h)
        {
            return new IntRect(x, y, x + w, y + h);
        }

        public static IntRect CreateWHCentered(int x, int y, int w, int h)
        {
            var intRect = new IntRect(x, y, x + w, y + h);
            return intRect.AddXY(-w / 2, -h / 2);
        }

        public static IntRect CreateFromStringKey(string key)
        {
            var pieces = key.Split('_');
            return new IntRect(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]));
        }

        public bool Equals(IntRect other)
        {
            return x1 == other.x1 && x2 == other.x2 && y1 == other.y1 && y2 == other.y2;
        }

        public override IntShape Clone(int x, int y)
        {
            return new IntRect(x1 + x, y1 + y, x2 + x, y2 + y);
        }

        public IntRect AddXY(int x, int y)
        {
            return new IntRect(x1 + x, y1 + y, x2 + x, y2 + y);
        }

        public override string ToString()
        {
            return x1 + "," + y1 + "," + x2 + "," + y2;
        }

        // A rectangle only counts as overlapping/colliding if it is inside another. Not if edges touch.
        // This simplifies collision code by allowing the "snap" position right outside collision to be where the edges touch
        public bool Overlaps(IntRect other)
        {
            if (x1 >= other.x2 || other.x1 >= x2) return false;
            if (y1 >= other.y2 || other.y1 >= y2) return false;
            return true;
        }

        // Unlike the above, this counts edge hits, since it's used for rect <=> irt collision checks which are more nuanced and require it
        public bool ContainsPoint(IntPoint point)
        {
            bool withinXBounds = point.x >= x1 && point.x <= x2;
            bool withinYBounds = point.y >= y1 && point.y <= y2;
            return withinXBounds && withinYBounds;
        }

        public override IntShape FlipX()
        {
            return IntRect.CreateWH(-x1 - w, y1, w, h);
        }

        public IntPoint GetCenter()
        {
            return new IntPoint(x1 + w / 2, y1 + h / 2);
        }
    }
}
