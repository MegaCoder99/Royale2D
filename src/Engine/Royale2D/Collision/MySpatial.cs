namespace Royale2D
{
    // REFACTOR remove spatial library
    public static class MySpatial
    {
        public static bool ShapesIntersect(IntShape myShape, IntShape theirShape)
        {
            bool intersects = false;
            if (myShape is IntRect myRect && theirShape is IntRect theirRect) intersects = RectsIntersect(myRect, theirRect);
            if (myShape is IntRect myRect2 && theirShape is IntIrt theirIrt2) intersects = RectIntersectsIrt(myRect2, theirIrt2);
            if (myShape is IntIrt myIrt3 && theirShape is IntRect theirRect3) intersects = RectIntersectsIrt(theirRect3, myIrt3);
            return intersects;
        }

        public static bool RectsIntersect(IntRect rect1, IntRect rect2)
        {
            return rect1.Overlaps(rect2);
        }

        public static List<IntPoint> GetRectIntersectPoints(IntRect rect1, IntRect rect2)
        {
            List<IntPoint> points = new List<IntPoint>();

            // Calculate the intersection rectangle
            int x1 = Math.Max(rect1.x1, rect2.x1);
            int y1 = Math.Max(rect1.y1, rect2.y1);
            int x2 = Math.Min(rect1.x2, rect2.x2);
            int y2 = Math.Min(rect1.y2, rect2.y2);

            // Check if there is an intersection
            if (x1 <= x2 && y1 <= y2)
            {
                // Add all corners of the intersection rectangle
                points.Add(new IntPoint(x1, y1));
                points.Add(new IntPoint(x1, y2));
                points.Add(new IntPoint(x2, y1));
                points.Add(new IntPoint(x2, y2));
            }

            return points;
        }

        public static IntPoint GetRectIntersectCenter(IntRect rect1, IntRect rect2)
        {
            List<IntPoint> points = GetRectIntersectPoints(rect1, rect2);
            return GetCenterPoint(points);
        }

        public static bool RectIntersectsIrt(IntRect rect, IntIrt irt)
        {
            bool foundContainment = false;
            foreach (IntPoint p in rect.GetPoints())
            {
                if (irt.ContainsPoint(p))
                {
                    foundContainment = true;
                    break;
                }
            }
            foreach (IntPoint p in irt.GetPoints())
            {
                if (rect.ContainsPoint(p))
                {
                    foundContainment = true;
                    break;
                }
            }

            // ContainsPoints counts edge hits but we do not want that. To rule out edge hits, do the containment rectangle overlap check too
            // PERF moving this check up to top of function might improve perf, but does it ruin logic?
            if (foundContainment)
            {
                IntRect irtBoundingRect = irt.GetBoundingRect();
                return rect.Overlaps(irtBoundingRect);
            }

            return false;
        }

        public static int GetIntersectArea(IntRect rect1, IntRect rect2)
        {
            if (!RectsIntersect(rect1, rect2)) return 0;

            IntRect? intersectRect = GetIntersectionRect(rect1, rect2);
            if (intersectRect != null)
            {
                return intersectRect.area;
            }

            return 0;
        }

        public static IntRect? GetIntersectionRect(IntRect rect1, IntRect rect2)
        {
            // Find the maximum of the left coordinates and the minimum of the right coordinates
            int intersectLeft = Math.Max(rect1.x1, rect2.x1);
            int intersectTop = Math.Max(rect1.y1, rect2.y1);
            int intersectRight = Math.Min(rect1.x2, rect2.x2);
            int intersectBottom = Math.Min(rect1.y2, rect2.y2);

            // If the rectangles intersect, the intersection coordinates must form a valid rectangle
            if (intersectLeft < intersectRight && intersectTop < intersectBottom)
            {
                return new IntRect(intersectLeft, intersectTop, intersectRight, intersectBottom);
            }

            return null;
        }

        public static IntPoint GetCenterPoint(List<IntPoint> points)
        {
            if (points.Count == 0) return IntPoint.Zero;

            int sumX = 0;
            int sumY = 0;
            foreach (IntPoint point in points)
            {
                sumX += point.x;
                sumY += point.y;
            }
            return new IntPoint(sumX / points.Count, sumY / points.Count);
        }
    }

    public struct ShapeIntersectData
    {
        public List<Point> intersectionPoints;
        public Point normal;

        public ShapeIntersectData(List<Point> intersectionPoints, Point normal)
        {
            this.intersectionPoints = intersectionPoints;
            this.normal = normal;
        }
    }
}
