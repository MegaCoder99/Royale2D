namespace Royale2D
{
    // IRT stands for isosceles right triangle. The only diagonal collision shape supported, for simplicity, perf and netcode determinism (avoids float/trig calculations).
    // All IRT's MUST have their right angles be aligned to x/y axes, thus there are only 4 possible configurations (determined by IntDir dir variable)
    public class IntIrt : IntShape
    {
        public IntPoint rightAnglePoint;
        public int legLength;
        public IrtDir dir;      // By convention, this is the orientation of the right angle of the IRT with respect to the triangle

        public IntIrt(IntPoint rightAnglePoint, int legLength, IrtDir dir)
        {
            this.rightAnglePoint = rightAnglePoint;
            this.legLength = legLength;
            this.dir = dir;

            _points.Add(rightAnglePoint);

            // Add the other two points based on the direction of the right angle
            switch (dir)
            {
                case IrtDir.TopLeft:
                    _points.Add(new IntPoint(rightAnglePoint.x + legLength, rightAnglePoint.y));
                    _points.Add(new IntPoint(rightAnglePoint.x, rightAnglePoint.y + legLength));
                    break;
                case IrtDir.TopRight:
                    _points.Add(new IntPoint(rightAnglePoint.x - legLength, rightAnglePoint.y));
                    _points.Add(new IntPoint(rightAnglePoint.x, rightAnglePoint.y + legLength));
                    break;
                case IrtDir.BottomLeft:
                    _points.Add(new IntPoint(rightAnglePoint.x + legLength, rightAnglePoint.y));
                    _points.Add(new IntPoint(rightAnglePoint.x, rightAnglePoint.y - legLength));
                    break;
                case IrtDir.BottomRight:
                    _points.Add(new IntPoint(rightAnglePoint.x - legLength, rightAnglePoint.y));
                    _points.Add(new IntPoint(rightAnglePoint.x, rightAnglePoint.y - legLength));
                    break;
            }
        }

        public override IntShape Clone(int xOff, int yOff)
        {
            return new IntIrt(rightAnglePoint.AddXY(xOff, yOff), legLength, dir);
        }

        public override string ToString()
        {
            return rightAnglePoint.x + "," + rightAnglePoint.y + ",leg=" + legLength + ",dir=" + dir.ToString();
        }

        public bool ContainsPoint(IntPoint point)
        {
            if (dir == IrtDir.TopLeft) return IsPointInIRTTopLeft(point);
            else if (dir == IrtDir.TopRight) return IsPointInIRTTopRight(point);
            else if (dir == IrtDir.BottomLeft) return IsPointInIRTBottomLeft(point);
            else return IsPointInIRTBottomRight(point);
        }

        public IntRect GetBoundingRect()
        {
            if (dir == IrtDir.TopLeft) return IntRect.CreateWH(rightAnglePoint.x, rightAnglePoint.y, legLength, legLength);
            else if (dir == IrtDir.TopRight) return IntRect.CreateWH(rightAnglePoint.x - legLength, rightAnglePoint.y, legLength, legLength);
            else if (dir == IrtDir.BottomLeft) return IntRect.CreateWH(rightAnglePoint.x, rightAnglePoint.y - legLength, legLength, legLength);
            else return IntRect.CreateWH(rightAnglePoint.x - legLength, rightAnglePoint.y - legLength, legLength, legLength);
        }

        // These counts edge hits, since it's used for rect <=> irt collision checks which are more nuanced and require it
        bool IsPointInIRTTopLeft(IntPoint p)
        {
            IntPoint topLeftPoint = rightAnglePoint;
            return p.x >= rightAnglePoint.x && p.y >= rightAnglePoint.y &&
                   p.x <= rightAnglePoint.x + legLength && p.y <= rightAnglePoint.y + legLength &&
                   (p.x - topLeftPoint.x) + (p.y - topLeftPoint.y) < legLength;
        }

        bool IsPointInIRTBottomRight(IntPoint p)
        {
            IntPoint topLeftPoint = new IntPoint(rightAnglePoint.x - legLength, rightAnglePoint.y - legLength);
            return p.x <= rightAnglePoint.x && p.y <= rightAnglePoint.y &&
                   p.x >= rightAnglePoint.x - legLength && p.y >= rightAnglePoint.y - legLength &&
                   (p.x - topLeftPoint.x) + (p.y - topLeftPoint.y) > legLength;
        }

        bool IsPointInIRTTopRight(IntPoint p)
        {
            IntPoint topRightPoint = rightAnglePoint;
            return p.x <= rightAnglePoint.x && p.y >= rightAnglePoint.y &&
                   p.x >= rightAnglePoint.x - legLength && p.y <= rightAnglePoint.y + legLength &&
                   (topRightPoint.x - p.x) + (p.y - topRightPoint.y) < legLength;
        }

        bool IsPointInIRTBottomLeft(IntPoint p)
        {
            IntPoint topRightPoint = new IntPoint(rightAnglePoint.x + legLength, rightAnglePoint.y - legLength);
            return p.x >= rightAnglePoint.x && p.y <= rightAnglePoint.y &&
                   p.x <= rightAnglePoint.x + legLength && p.y >= rightAnglePoint.y - legLength &&
                   (topRightPoint.x - p.x) + (p.y - topRightPoint.y) > legLength;
        }

        public override IntShape FlipX()
        {
            // Not implemented for now as nowhere needs it
            return this;
        }
    }

    public enum IrtDir
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
