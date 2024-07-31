namespace NITHlibrary.Tools.Filters.ValueFilters
{
    /// <summary>
    /// Interface for filters that apply to double values.
    /// </summary>
    public interface IDoubleFilter
    {
        /// <summary>
        /// Feeds a new input value to the filter.
        /// </summary>
        void Push(double value);

        /// <summary>
        /// Computes the filtered output value.
        /// </summary>
        /// <returns>The filtered output value.</returns>
        double Pull();
    }
}
