using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    /// <summary>
    /// A filter which does... Nothing! Output = input.
    /// </summary>
    public class PointFilterBypass : IPointFilter
    {
        private Point point;

        public PointFilterBypass()
        {
            point = new Point(0, 0);
        }

        public void Push(Point point)
        {
            this.point = point;
        }

        public Point GetOutput()
        {
            return point;
        }
    }
}
