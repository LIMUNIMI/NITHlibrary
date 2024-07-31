using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// Implements an exponentially decaying moving average filter for Point objects.
    /// This filter reduces the impact of older points over time based on the specified decay factor.
    /// </summary>
    public class PointFilterMAexpDecaying : IPointFilter
    {
        private Point _pointI = new(0, 0);
        private Point _pointIplusOne = new(0, 0);
        private readonly float _alpha;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFilterMAexpDecaying"/> class.
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.
        /// The higher this value, the faster the response will be over time. 
        /// The lower this value, the slower but more smooth will be the response over time.</param>
        public PointFilterMAexpDecaying(float alpha)
        {
            this._alpha = alpha;
        }

        /// <summary>
        /// Pushes a new point into the filter, updating the moving average.
        /// </summary>
        /// <param name="point">The new point to be pushed into the filter.</param>
        public void Push(Point point)
        {
            _pointI.X = _pointIplusOne.X;
            _pointI.Y = _pointIplusOne.Y;
            _pointIplusOne.X = (int)(_alpha * point.X) + (int)((1 - _alpha) * _pointI.X);
            _pointIplusOne.Y = (int)(_alpha * point.Y) + (int)((1 - _alpha) * _pointI.Y);
        }

        /// <summary>
        /// Gets the filtered output point based on the moving average.
        /// </summary>
        /// <returns>The current output point of the filter.</returns>
        public Point GetOutput()
        {
            return new Point(_pointIplusOne.X, _pointIplusOne.Y);
        }
    }
}