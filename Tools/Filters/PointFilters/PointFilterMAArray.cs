using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// An array-based moving average filter. Calculates the average of the last [arrayDimension] points.
    /// </summary>
    public class PointFilterMAArray : IPointFilter
    {
        private Point[] points;

        public PointFilterMAArray(int arrayDimension)
        {
            points = new Point[arrayDimension];
        }

        public void Push(Point point)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                points[i + 1] = points[i];
            }

            points[0] = point;
        }

        public Point GetOutput()
        {
            int x = 0;
            int y = 0;
            foreach (Point point in points)
            {
                x += point.X;
                y += point.Y;
            }
            x = x / points.Length;
            y = y / points.Length;
            return new Point(x, y);
        }
    }
}
