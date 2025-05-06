using System.Runtime.Serialization;

namespace Royale2D
{
    public struct FdPoint
    {
        public Fd x = new Fd();
        public Fd y = new Fd();

        public static FdPoint Zero => new FdPoint(0, 0);

        public FdPoint()
        {
        }

        public FdPoint(Fd x, Fd y)
        {
            this.x = x;
            this.y = y;
        }

        public static FdPoint FromXY(int x, int y)
        {
            return new FdPoint(Fd.New(x), Fd.New(y));
        }

        public Point ToFloatPoint() => new Point(x.floatVal, y.floatVal);

        public IntPoint ToIntPoint() => new IntPoint(x.intVal, y.intVal);

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

            FdPoint other = (FdPoint)obj;
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override int GetHashCode()
        {
            // Use a combination of the hash codes of the individual components
            return HashCode.Combine(x.GetHashCode(), y.GetHashCode());
        }

        public static bool operator ==(FdPoint left, FdPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FdPoint left, FdPoint right)
        {
            return !left.Equals(right);
        }

        public FdPoint AddXY(Fd x, Fd y)
        {
            var point = new FdPoint(this.x + x, this.y + y);
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

        public Fd DistanceTo(FdPoint other)
        {
            LongFd deltaX = (other.x - x).longFd;
            LongFd deltaY = (other.y - y).longFd;
            return NetcodeSafeMath.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        // Faster than DistanceTo, but not as accurate
        public Fd RoughDistTo(FdPoint other)
        {
            return (other.x - x).abs + (other.y - y).abs;
        }

        // Squared euclidean distance, faster than DistanceTo, useful for relative comparisons
        public LongFd SqeuDistanceTo(FdPoint other)
        {
            LongFd deltaX = (other.x - x).longFd;
            LongFd deltaY = (other.y - y).longFd;
            return deltaX * deltaX + deltaY * deltaY;
        }

        public Fd Magnitude()
        {
            LongFd longX = x.longFd;
            LongFd longY = y.longFd;
            return NetcodeSafeMath.Sqrt(longX * longX + longY * longY);
        }

        public FdPoint DirTo(FdPoint pos)
        {
            return pos - this;
        }

        public FdPoint DirToNormalized(FdPoint pos)
        {
            return (pos - this).Normalized();
        }

        public FdPoint Normalized()
        {
            Fd angle = NetcodeSafeMath.ArcTanD(y, x);
            return new FdPoint(NetcodeSafeMath.CosD(angle), NetcodeSafeMath.SinD(angle));
        }

        public FdPoint WithoutComponent(FdPoint other)
        {
            return this - Project(other);
        }

        private static LongFd DotProduct(FdPoint a, FdPoint b)
        {
            return (a.x.longFd * b.x.longFd) + (a.y.longFd * b.y.longFd);
        }

        // Projects this FdPoint onto another FdPoint 'other'
        public FdPoint Project(FdPoint other)
        {
            LongFd dotProduct = DotProduct(this, other);
            LongFd denominator = DotProduct(other, other);
            if (denominator.internalVal == 0) return new FdPoint(0, 0);  // Prevent division by zero

            Fd scalar = (dotProduct / denominator).fd;

            // Return the projection of this vector onto 'other'
            return new FdPoint(scalar * other.x, scalar * other.y);
        }

        public FdPoint IncMag(Fd incAmount)
        {
            FdPoint norm = Normalized();
            norm *= incAmount;
            return new FdPoint(x + norm.x, y + norm.y);
        }

        #region operators
        public static FdPoint operator +(FdPoint point1, FdPoint point2)
        {
            return new FdPoint(point1.x + point2.x, point1.y + point2.y);
        }

        public static FdPoint operator -(FdPoint point1, FdPoint point2)
        {
            return new FdPoint(point1.x - point2.x, point1.y - point2.y);
        }

        public static FdPoint operator *(FdPoint point1, int val)
        {
            return new FdPoint(point1.x * val, point1.y * val);
        }

        public static FdPoint operator *(FdPoint point1, Fd val)
        {
            return new FdPoint(point1.x * val, point1.y * val);
        }

        public static FdPoint operator /(FdPoint point1, int val)
        {
            return new FdPoint(point1.x / val, point1.y / val);
        }

        public static FdPoint operator /(FdPoint point1, Fd val)
        {
            return new FdPoint(point1.x / val, point1.y / val);
        }
        #endregion
    }
}
