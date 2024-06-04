using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// Outputs the average of the last [arrayDimension] points, but discards the "outsiders": namely, those points for which the difference with the last calculated mean exceeds [maxVariation]. Can increase the stability of a noisy signal.
    /// </summary>
    public class PointFilterMAArrayOutsiders : IPointFilter
    {
        private Point[] points;
        private Point lastMean = new Point(0, 0);
        private int maxVariation;

        public PointFilterMAArrayOutsiders(int arrayDimension, int maxVariation)
        {
            points = new Point[arrayDimension];
            this.maxVariation = maxVariation;
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
                if (Math.Abs(x - lastMean.X) < maxVariation)
                {
                    x += point.X;
                }
                if (Math.Abs(y - lastMean.Y) < maxVariation)
                {
                    y += point.Y;
                }
            }
            x = x / points.Length;
            y = y / points.Length;
            lastMean.X = x;
            lastMean.Y = y;
            return new Point(x, y);
        }
    }
}
