namespace NITHlibrary.Tools.Filters.ArrayFilters
{
    /// <summary>
    /// Interface for filters that apply to double arrays.
    /// </summary>
    public interface IDoubleArrayFilter
    {
        /// <summary>
        /// Feeds a new input array to the filter.
        /// </summary>
        /// <param name="value"></param>
        void Push(double[] value);

        /// <summary>
        /// Retrieves the filtered output array.
        /// </summary>
        /// <returns></returns>
        double[] Pull();
    }
}
