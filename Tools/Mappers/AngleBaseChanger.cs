namespace NITHlibrary.Tools.Mappers
{
    /// <summary>
    /// Does your gyroscope or whatever return you angles in [-180; +180]° or [0; +360]° format, and you want to move the center wherever you desire?
    /// Here I am!
    /// </summary>
    public class AngleBaseChanger
    {
        private double deltaBar = 180;
        private double delta = 0;

        /// <summary>
        /// Defines the new rotated base. Delta is the angle that your sensor read when your gyro is in the desired center.
        /// </summary>
        public double Delta
        {
            get { return delta; }
            set
            {
                delta = value;
                if (value >= 0)
                {
                    deltaBar = -180 + value;
                }
                else
                {
                    deltaBar = 180 + value;
                }
            }
        }

        /// <summary>
        /// Converts a sensor read into the new base defined by delta. Output will be in [-180; +180]° format.
        /// </summary>
        /// <param name="angle">The angle read by the sensor</param>
        /// <returns></returns>
        public double Transform(double angle)
        {
            // Put angle in [-180;+180] format
            if (angle >= 180)
            {
                angle -= 360;
            }
            else if (angle <= -180)
            {
                angle += 360;
            }

            // Transformations

            double res = 0;
            if (delta >= 0)
            {
                if (angle > deltaBar)
                {
                    res = angle - delta;
                }
                else
                {
                    res = 180 + angle - deltaBar;
                }
            }
            else if (delta < 0)
            {
                if (angle < deltaBar)
                {
                    res = angle - delta;
                }
                else
                {
                    res = -180 + angle - deltaBar;
                }
            }
            return res;
        }





    }
}
