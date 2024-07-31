using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters;

/// <summary>
/// Represents a point filter that returns the input point without any modification.
/// </summary>
public class PointFilterBypass : IPointFilter
{
    private Point _point = new(0, 0);

    /// <summary>
    /// Accepts a point input and stores it.
    /// </summary>
    /// <param name="point">The input point.</param>
    public void Push(Point point)
    {
        this._point = point;
    }

    /// <summary>
    /// Returns the stored point, which is the same as the input.
    /// </summary>
    /// <returns>The output point, which is identical to the input point.</returns>
    public Point GetOutput()
    {
        return _point;
    }
}