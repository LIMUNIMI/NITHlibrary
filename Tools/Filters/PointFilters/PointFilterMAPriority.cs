using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// Specifies the types of decreasing functions that can be used for weighting the points in the filter.
    /// </summary>
    public enum DecreasingFunction
    {
        Linear,
        Quadratic
    };

    /// <summary>
    /// An array-based moving average filter. The "priority" of the points in the array decreases
    /// at each step, following a (selectable) linear or quadratic function.
    /// </summary>
    public class PointFilterMApriority : IPointFilter
    {
        private readonly DecreasingFunction _function;
        private readonly int _numberOfPoints;
        private readonly List<KeyValuePair<Point, int>> _pointsWithPriority = [];
        private readonly int _weights = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointFilterMApriority"/> class
        /// with the specified number of points and decreasing function.
        /// </summary>
        /// <param name="numberOfPoints">The number of points to be stored for moving average calculation.</param>
        /// <param name="function">The decreasing function to be used for weight calculation.</param>
        public PointFilterMApriority(int numberOfPoints, DecreasingFunction function)
        {
            this._numberOfPoints = numberOfPoints;
            this._function = function;

            for (var i = 0; i < numberOfPoints; i++)
            {
                _pointsWithPriority.Add(new(new(0, 0), numberOfPoints));
            }

            for (var i = 0; i < numberOfPoints; i++)
            {
                switch (_function)
                {
                    case DecreasingFunction.Linear:
                        _weights += i;
                        break;

                    case DecreasingFunction.Quadratic:
                        _weights += i * i;
                        break;
                }
            }

            Console.WriteLine(_weights);
        }

        /// <summary>
        /// Computes and returns the weighted mean of the points based on their priorities.
        /// </summary>
        /// <returns>A <see cref="Point"/> representing the weighted mean of the stored points.</returns>
        public Point GetOutput()
        {
            Point weightedMean = new(0, 0);

            switch (_function)
            {
                case DecreasingFunction.Linear:
                    {
                        for (var i = 0; i < _numberOfPoints; i++)
                        {
                            weightedMean.X += _pointsWithPriority[i].Key.X * _pointsWithPriority[i].Value;
                            weightedMean.Y += _pointsWithPriority[i].Key.Y * _pointsWithPriority[i].Value;
                        }

                        weightedMean.X /= _weights;
                        weightedMean.Y /= _weights;
                        break;
                    }
                case DecreasingFunction.Quadratic:
                    {
                        for (var i = 0; i < _numberOfPoints; i++)
                        {
                            int quadraticWeight = _pointsWithPriority[i].Value * _pointsWithPriority[i].Value;
                            weightedMean.X += _pointsWithPriority[i].Key.X * quadraticWeight;
                            weightedMean.Y += _pointsWithPriority[i].Key.Y * quadraticWeight;
                        }

                        weightedMean.X /= _weights;
                        weightedMean.Y /= _weights;
                        break;
                    }
            }

            return weightedMean;
        }

        /// <summary>
        /// Adds a new point to the filter and updates the priority of existing points.
        /// </summary>
        /// <param name="point">The new point to be added.</param>
        public void Push(Point point)
        {
            for (var i = 0; i < _numberOfPoints - 1; i++)
            {
                _pointsWithPriority[i + 1] = new(_pointsWithPriority[i].Key, _pointsWithPriority[i].Value - 1);
            }

            _pointsWithPriority[0] = new(point, _numberOfPoints);
        }
    }
}