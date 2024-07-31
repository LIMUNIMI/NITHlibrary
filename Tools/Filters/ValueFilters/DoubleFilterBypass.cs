namespace NITHlibrary.Tools.Filters.ValueFilters
{
    /// <summary>
    /// A filter which does nothing. The output is equal to the input.
    /// </summary>
    public class DoubleFilterBypass : IDoubleFilter
    {
        private double _val = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleFilterBypass"/> class.
        /// </summary>
        public DoubleFilterBypass()
        {

        }

        /// <summary>
        /// Pushes a new value into the filter.
        /// </summary>
        /// <param name="val">The value to be pushed into the filter.</param>
        public void Push(double val)
        {
            this._val = val;
        }

        /// <summary>
        /// Pulls the current value from the filter (which will be the same as the last input).
        /// </summary>
        /// <returns>The current value in the filter.</returns>
        public double Pull()
        {
            return _val;
        }
    }
}