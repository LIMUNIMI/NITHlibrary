using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// Array based point filter (FIFO logic).
    /// Outputs the average of the last <c>arrayDimension</c>points, but discards the "outsiders": 
    /// namely, those points for which the difference with the last calculated mean exceeds <c>maxVariation</c>. 
    /// Can increase the stability of a noisy signal.
    /// </summary>
    public class PointFilterMAarrayOutsiders : IPointFilter
    {
        private readonly Point[] _points;
        private Point _lastMean = new(0, 0);
        private readonly int _maxVariation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFilterMAarrayOutsiders"/> class.
        /// </summary>
        /// <param name="arrayDimension">The dimension of the array to hold the points.</param>
        /// <param name="maxVariation">The maximum allowed variation from the last mean for a point to be considered.</param>
        public PointFilterMAarrayOutsiders(int arrayDimension, int maxVariation)
        {
            _points = new Point[arrayDimension];
            this._maxVariation = maxVariation;
        }

        /// <summary>
        /// Inserts a new point into the filter's internal array by shifting older points and removing the first one.
        /// </summary>
        /// <param name="point">The new point to be added.</param>
        public void Push(Point point)
        {
            for (var i = 0; i < _points.Length - 1; i++)
            {
                _points[i + 1] = _points[i];
            }
            _points[0] = point;
        }

        /// <summary>
        /// Computes and returns the filtered output point.
        /// </summary>
        /// <returns>A <see cref="Point"/> representing the filtered output.</returns>
        public Point GetOutput()
        {
            var x = 0;
            var y = 0;
            foreach (var point in _points)
            {
                if (Math.Abs(x - _lastMean.X) < _maxVariation)
                {
                    x += point.X;
                }
                if (Math.Abs(y - _lastMean.Y) < _maxVariation)
                {
                    y += point.Y;
                }
            }
            x = x / _points.Length;
            y = y / _points.Length;
            _lastMean.X = x;
            _lastMean.Y = y;
            return new(x, y);
        }
    }
}