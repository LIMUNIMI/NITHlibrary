using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// An array-based moving average filter (FIFO logic).
    /// Calculates the average of the last [arrayDimension] points.
    /// </summary>
    public class PointFilterMAarray : IPointFilter
    {
        private readonly Point[] _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFilterMAarray"/> class with the specified array dimension.
        /// </summary>
        /// <param name="arrayDimension">The number of points over which to calculate the moving average.</param>
        public PointFilterMAarray(int arrayDimension)
        {
            _points = new Point[arrayDimension];
        }

        /// <summary>
        /// Adds a new point to the array and updates the moving average.
        /// </summary>
        /// <param name="point">The point to add to the moving average calculation.</param>
        public void Push(Point point)
        {
            for (var i = 0; i < _points.Length - 1; i++)
            {
                _points[i + 1] = _points[i];
            }

            _points[0] = point;
        }

        /// <summary>
        /// Gets the current moving average point..
        /// </summary>
        /// <returns>The current moving average point.</returns>
        public Point GetOutput()
        {
            var x = 0;
            var y = 0;
            foreach (var point in _points)
            {
                x += point.X;
                y += point.Y;
            }
            x = x / _points.Length;
            y = y / _points.Length;
            return new(x, y);
        }
    }
}