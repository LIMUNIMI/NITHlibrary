using System.Drawing;

namespace NITHlibrary.Tools.Filters.PointFilters
{
    public class PointFilterMAExpDecaying : IPointFilter
    {
        private Point PointI = new Point(0, 0);
        private Point PointIplusOne = new Point(0, 0);
        private float alpha;

        /// <summary>
        /// The classic implementation of an exponentially decaying moving average filter. 
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.</param>
        public PointFilterMAExpDecaying(float alpha)
        {
            this.alpha = alpha;
        }

        public void Push(Point point)
        {
            PointI.X = PointIplusOne.X;
            PointI.Y = PointIplusOne.Y;
            PointIplusOne.X = (int)(alpha * point.X) + (int)((1 - alpha) * PointI.X);
            PointIplusOne.Y = (int)(alpha * point.Y) + (int)((1 - alpha) * PointI.Y);
        }

        public Point GetOutput()
        {

            return new Point(PointIplusOne.X, PointIplusOne.Y);
        }
    }
}
