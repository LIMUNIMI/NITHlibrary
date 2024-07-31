using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// An interface for a generic point filter.
    /// </summary>
    public interface IPointFilter
    {
        /// <summary>
        /// Inserts a new point in the filter's array.
        /// </summary>
        void Push(Point point);

        /// <summary>
        /// Returns the output value from the filter.
        /// </summary>
        /// <returns></returns>
        Point GetOutput();
    }
}
