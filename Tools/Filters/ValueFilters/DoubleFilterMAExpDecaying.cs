namespace NITHlibrary.Tools.Filters.ValueFilters
{
    /// <summary>
    /// Represents a filter that applies an exponentially decaying moving average to a series of double values.
    /// More recent values are given more weight and older values decay exponentially, contributing less as they get older.
    /// </summary>
    public class DoubleFilterMAexpDecaying : IDoubleFilter
    {
        private double _valI;
        private double _valIplusOne;
        private readonly float _alpha;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleFilterMAexpDecaying"/> class.
        /// </summary>
        /// <param name="alpha">Determines the weighting factor, controlling the speed of decay for older values.
        /// The higher this value, the faster the response will be over time. The lower this value, the slower but more smooth will be the response over time.</param>
        public DoubleFilterMAexpDecaying(float alpha)
        {
            this._alpha = alpha;
        }

        /// <summary>
        /// Adds a new value to the filter and updates the internal state accordingly.
        /// </summary>
        /// <param name="val">The new value to be added to the filter.</param>
        public void Push(double val)
        {
            if (double.IsNaN(val)) return;
            _valI = _valIplusOne;
            _valIplusOne = _alpha * val + (1 - _alpha) * _valI;
        }

        /// <summary>
        /// Retrieves the current filtered value.
        /// </summary>
        /// <returns>The current value after applying the exponentially decaying moving average filter.</returns>
        public double Pull()
        {
            return _valIplusOne;
        }
    }
}