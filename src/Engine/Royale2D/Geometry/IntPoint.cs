namespace Royale2D
{
    public struct IntPoint
    {
        public int x;
        public int y;

        public static IntPoint Zero => new IntPoint(0, 0);

        public IntPoint()
        {
        }

        public IntPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point ToFloatPoint() => new Point(x, y);

        public FdPoint ToFdPoint() => new FdPoint(x, y);

        public override string ToString()
        {
            return x + "," + y;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            IntPoint other = (IntPoint)obj;
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override int GetHashCode()
        {
            // Use a combination of the hash codes of the individual components
            return HashCode.Combine(x.GetHashCode(), y.GetHashCode());
        }

        public IntPoint AddXY(int x, int y)
        {
            var point = new IntPoint(this.x + x, this.y + y);
            return point;
        }

        public bool IsZero()
        {
            return x == 0 && y == 0;
        }

        public bool IsNonZero()
        {
            return x != 0 || y != 0;
        }

        // SQEU stands for Squared Euclidean
        public long SqeuDistanceTo(IntPoint other)
        {
            long deltaX = (other.x - x);
            long deltaY = (other.y - y);
            return deltaX * deltaX + deltaY * deltaY;
        }

        public static IntPoint operator +(IntPoint point1, IntPoint point2)
        {
            return new IntPoint(point1.x + point2.x, point1.y + point2.y);
        }

        public static IntPoint operator -(IntPoint point1, IntPoint point2)
        {
            return new IntPoint(point1.x - point2.x, point1.y - point2.y);
        }

        public static IntPoint operator *(IntPoint point1, int val)
        {
            return new IntPoint(point1.x * val, point1.y * val);
        }

        public static bool operator ==(IntPoint point1, IntPoint point2)
        {
            return point1.x == point2.x && point1.y == point2.y;
        }

        public static bool operator !=(IntPoint point1, IntPoint point2)
        {
            return point1.x != point2.x || point1.y != point2.y;
        }
    }
}
