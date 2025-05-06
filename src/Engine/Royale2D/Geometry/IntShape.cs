namespace Royale2D
{
    public abstract class IntShape
    {
        protected List<IntPoint> _points = new List<IntPoint>();

        // Don't modify this, it will have no effect. If you need to modify it, create a new shape instead
        public List<IntPoint> GetPoints() => _points;

        public virtual IntShape Clone(int x, int y)
        {
            throw new NotImplementedException();
        }

        public IntPoint Center()
        {
            long sumX = 0;
            long sumY = 0;
            foreach (IntPoint point in _points)
            {
                sumX += point.x;
                sumY += point.y;
            }
            return new IntPoint((int)(sumX / _points.Count), (int)(sumY / _points.Count));
        }

        public long SqeuDistanceTo(IntShape other)
        {
            return Center().SqeuDistanceTo(other.Center());
        }

        public int MinY()
        {
            int min = _points[0].y;
            foreach (IntPoint point in _points)
            {
                if (point.y < min) min = point.y;
            }
            return min;
        }

        public int MaxY()
        {
            int max = _points[0].y;
            foreach (IntPoint point in _points)
            {
                if (point.y > max) max = point.y;
            }
            return max;
        }

        public int MinX()
        {
            int min = _points[0].x;
            foreach (IntPoint point in _points)
            {
                if (point.x < min) min = point.x;
            }
            return min;
        }

        public int MaxX()
        {
            int max = _points[0].x;
            foreach (IntPoint point in _points)
            {
                if (point.x > max) max = point.x;
            }
            return max;
        }

        // -1 means no overlap, 0 means overlap right on edge
        public int GetOverlapX(IntShape other)
        {
            int ax1 = MinX();
            int ax2 = MaxX();
            int bx1 = other.MinX();
            int bx2 = other.MaxX();

            int start = Math.Max(ax1, bx1); // Start of the potential overlap
            int end = Math.Min(ax2, bx2); // End of the potential overlap

            // If the end of the overlap is before the start, there's no overlap
            if (end < start) return -1;

            // If the end equals start, it means they are touching at the edge without overlapping
            if (end == start) return 0;

            // Otherwise, the overlap distance is the difference between end and start
            return end - start;
        }

        // -1 means no overlap, 0 means overlap right on edge
        public int GetOverlapY(IntShape other)
        {
            int ay1 = MinY();
            int ay2 = MaxY();
            int by1 = other.MinY();
            int by2 = other.MaxY();

            int start = Math.Max(ay1, by1); // Start of the potential overlap
            int end = Math.Min(ay2, by2); // End of the potential overlap

            // If the end of the overlap is before the start, there's no overlap
            if (end < start) return -1;

            // If the end equals start, it means they are touching at the edge without overlapping
            if (end == start) return 0;

            // Otherwise, the overlap distance is the difference between end and start
            return end - start;
        }

        public virtual IntShape FlipX()
        {
            throw new NotImplementedException();
        }

        public bool EqualTo(IntShape other)
        {
            List<IntPoint> myPoints = GetPoints();
            List<IntPoint> otherPoints = other.GetPoints();
            if (myPoints.Count != otherPoints.Count) return false;
            for (int i = 0; i < myPoints.Count; i++)
            {
                if (myPoints[i] != otherPoints[i]) return false;
            }
            return true;
        }
    }
}
