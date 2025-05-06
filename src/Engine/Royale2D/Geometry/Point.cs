using Shared;

namespace Royale2D
{
    // REFACTOR rename to FloatPoint, but bear in mind, using statements/namespaces could get screwed up due to .NET's Point type
    public struct Point
    {
        public float x;
        public float y;

        public static Point Zero => new Point(0, 0);

        public Point()
        {
            x = 0;
            y = 0;
        }

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Point other = (Point)obj;
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override int GetHashCode()
        {
            // Use a combination of the hash codes of the individual components
            return HashCode.Combine(x.GetHashCode(), y.GetHashCode());
        }

        public float magnitude
        {
            get
            {
                if (x == 0) return MathF.Abs(y);
                if (y == 0) return MathF.Abs(x);
                var root = x * x + y * y;
                if (root < 0) root = 0;
                var result = MathF.Sqrt(root);
                if (float.IsNaN(result)) throw new Exception("NAN!");
                return result;
            }
        }

        public Point normalized
        {
            get
            {
                if (IsZero()) return Zero;
                float mag = magnitude;
                return new Point(x / mag, y / mag);
            }
        }

        public Point AddXY(float x, float y)
        {
            var point = new Point(this.x + x, this.y + y);
            return point;
        }

        public Point DirTo(Point point)
        {
            return (point - (this)).normalized;
        }

        public bool IsNonZero()
        {
            return x != 0 || y != 0;
        }

        public bool IsZero()
        {
            return !IsNonZero();
        }

        // Returns new point
        public Point Add(Point other)
        {
            var point = new Point(x + other.x, y + other.y);
            return point;
        }

        // Mutates this point
        public void Inc(Point other)
        {
            x += other.x;
            y += other.y;
        }

        //Returns new point
        public Point Times(float num)
        {
            var point = new Point(x * num, y * num);
            return point;
        }

        //Mutates this point
        public Point Multiply(float num)
        {
            x *= num;
            y *= num;
            return this;
        }

        public float DistanceTo(Point other)
        {
            return MathF.Sqrt(MathF.Pow(other.x - x, 2) + MathF.Pow(other.y - y, 2));
        }

        public static Point Average(List<Point> points)
        {
            if (points.Count == 0) return new Point();
            Point sum = new Point();
            foreach (var point in points)
            {
                sum.Inc(point);
            }
            return sum.Multiply(1.0f / points.Count);
        }

        public Point ChangeMagnitude(float newMagnitude)
        {
            return normalized.Times(newMagnitude);
        }

        public Point UnitInc(float num)
        {
            return Add(normalized.Times(num));
        }

        public float ix => (float)Math.Round(x);
        public float iy => (float)Math.Round(y);

        public float angle => (float)Math.Atan2(y, x);

        public Point Times(int value)
        {
            return new Point(x * value, y * value);
        }

        public Point ChangeX(float x)
        {
            return new Point(x, y);
        }

        public Point ChangeY(float y)
        {
            return new Point(x, y);
        }

        public void normalize()
        {
            Point normalizedPoint = normalized;
            x = normalizedPoint.x;
            y = normalizedPoint.y;
        }

        public Point incMag(float incAmount)
        {
            Point norm = normalized;
            norm *= incAmount;
            return new Point(x + norm.x, y + norm.y);
        }

        public float GetAngleInDegreesClamped()
        {
            var ang = MathF.Atan2(y, x);
            ang *= 180 / MathF.PI;
            if (ang < 0) ang += 360;
            return ang;
        }

        public float AngleWith(Point other)
        {
            float ang1 = angle;
            float ang2 = other.angle;
            if (ang1 < 0) ang1 += (float)Math.PI * 2;
            if (ang2 < 0) ang2 += (float)Math.PI * 2;
            if (ang1 >= (float)Math.PI * 2) ang1 -= (float)Math.PI * 2;
            if (ang2 >= (float)Math.PI * 2) ang2 -= (float)Math.PI * 2;
            float ang = Math.Abs(ang1 - ang2);
            return ang;
        }

        public Point rayTo(Point point)
        {
            return (point - (this));
        }

        public float distTo(Point point)
        {
            return (float)Math.Sqrt(Math.Pow(point.x - x, 2) + (float)Math.Pow(point.y - y, 2));
        }


        //public Point Project(Point other)
        //{
        //    float ang = angle(other);
        //    return other.normalized * (magnitude * (float)Math.Cos(ang));
        //}

        // More efficient than above
        public Point Project(Point other)
        {
            var dp = dotProduct(other);
            return new Point((dp / (other.x * other.x + other.y * other.y)) * other.x, (dp / (other.x * other.x + other.y * other.y)) * other.y);
        }

        public Point withoutComponent(Point other)
        {
            return this - Project(other);
        }

        public Point right()
        {
            return new Point(-y, x);
        }

        public string toString()
        {
            return x.ToString() + "," + y.ToString();
        }

        public static Point lerp(Point a, Point b, float t)
        {
            return a.Add(b.subtract(a).Times(t));
        }

        public static Point moveTo(Point a, Point b, float amount)
        {
            if (a.DistanceTo(b) <= amount * 2)
            {
                return b;
            }
            Point dirTo = a.directionToNorm(b);
            return a.Add(dirTo.Times(amount));
        }

        public float dotProduct(Point other)
        {
            return (x * other.x) + (y * other.y);
        }

        public Point leftOrRightNormal(int dir)
        {
            if (dir == 1) return rightNormal();
            else return leftNormal();
        }

        public Point rightNormal()
        {
            return new Point(-y, x);
        }

        public Point leftNormal()
        {
            return new Point(y, -x);
        }

        public float perProduct(Point other)
        {
            return dotProduct(other.rightNormal());
        }

        public static Point createFromAngle(float angle)
        {
            float x = MyMath.CosD(angle);
            float y = MyMath.SinD(angle);
            return new Point(x, y);
        }

        public float angleWith(Point other)
        {
            var ang = MathF.Atan2(other.y, other.x) - MathF.Atan2(y, x);
            ang *= 180 / MathF.PI;
            if (ang < 0) ang += 360;
            if (ang > 180) ang = 360 - ang;
            return ang;
        }

        public Point clone()
        {
            return new Point(x, y);
        }

        public bool isCloseToZero(float epsilon = 0.1f)
        {
            return magnitude < epsilon;
        }

        public Point subtract(Point other)
        {
            return new Point(x - other.x, y - other.y);
        }

        public Point directionTo(Point other)
        {
            return new Point(other.x - x, other.y - y);
        }

        public Point directionToNorm(Point other)
        {
            return (new Point(other.x - x, other.y - y)).normalized;
        }

        public bool isAngled()
        {
            return x != 0 && y != 0;
        }

        public override string ToString()
        {
            return x.ToString("0.0") + "," + y.ToString("0.0");
        }

        public static float minX(List<Point> points)
        {
            float minX = float.MaxValue;
            foreach (var point in points)
            {
                if (point.x < minX) minX = point.x;
            }
            return minX;
        }

        public static float maxX(List<Point> points)
        {
            float maxX = float.MinValue;
            foreach (var point in points)
            {
                if (point.x > maxX) maxX = point.x;
            }
            return maxX;
        }

        public static float minY(List<Point> points)
        {
            float minY = float.MaxValue;
            foreach (var point in points)
            {
                if (point.y < minY) minY = point.y;
            }
            return minY;
        }

        public static float maxY(List<Point> points)
        {
            float maxY = float.MinValue;
            foreach (var point in points)
            {
                if (point.y > maxY) maxY = point.y;
            }
            return maxY;
        }

        public bool isSideways()
        {
            return MathF.Abs(x) > MathF.Abs(y);
        }

        public bool isGroundNormal()
        {
            return y < 0 && MathF.Abs(y) > MathF.Abs(x);
        }

        public bool isCeilingNormal()
        {
            return y > 0 && MathF.Abs(y) > MathF.Abs(x);
        }

        public bool equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public Point closestPoint(List<Point> points)
        {
            if (points.Count == 0) return this;
            if (points.Count == 1) return points[0];

            Point bestPoint = this;
            float bestDist = float.MaxValue;
            foreach (var point in points)
            {
                float dist = DistanceTo(point);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPoint = point;
                }
            }

            return bestPoint;
        }

        public FdPoint ToFdPoint()
        {
            return new FdPoint(Fd.New(x), Fd.New(y));
        }

        #region operators
        public static Point operator +(Point point1, Point point2)
        {
            return new Point(point1.x + point2.x, point1.y + point2.y);
        }

        public static Point operator -(Point point1, Point point2)
        {
            return new Point(point1.x - point2.x, point1.y - point2.y);
        }

        public static Point operator *(Point point1, float val)
        {
            return new Point(point1.x * val, point1.y * val);
        }

        public static Point operator /(Point point1, float val)
        {
            return new Point(point1.x / val, point1.y / val);
        }

        public static bool operator ==(Point point1, Point point2)
        {
            return point1.x == point2.x && point1.y == point2.y;
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return point1.x != point2.x || point1.y != point2.y;
        }
        #endregion
    }
}
