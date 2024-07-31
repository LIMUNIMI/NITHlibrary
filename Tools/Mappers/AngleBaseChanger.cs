namespace NITHlibrary.Tools.Mappers
{
    /// <summary>
    /// Provides functionality to adjust angle measurements returned by sensors such as gyroscopes. 
    /// Converts sensor-read angles to a new reference system, [-180; +180]°, and makes it possible to change the center reference.
    /// This is useful, for example,to calibrate the gyroscope center.
    /// </summary>
    public class AngleBaseChanger
    {
        private double _deltaBar = 180;
        private double _delta;

        /// <summary>
        /// Gets or sets the angle offset used to determine the new center for angle calculations.
        /// </summary>
        public double Delta
        {
            get => _delta;
            set
            {
                _delta = value;
                if (value >= 0)
                {
                    _deltaBar = -180 + value;
                }
                else
                {
                    _deltaBar = 180 + value;
                }
            }
        }

        /// <summary>
        /// Converts an angle measurement read by the sensor to the new base defined by <see cref="Delta"/>.
        /// The output angle will be in the range of [-180; +180] degrees.
        /// </summary>
        /// <param name="angle">The angle read by the sensor.</param>
        /// <returns>The transformed angle in the new base.</returns>
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
            if (_delta >= 0)
            {
                if (angle > _deltaBar)
                {
                    res = angle - _delta;
                }
                else
                {
                    res = 180 + angle - _deltaBar;
                }
            }
            else if (_delta < 0)
            {
                if (angle < _deltaBar)
                {
                    res = angle - _delta;
                }
                else
                {
                    res = -180 + angle - _deltaBar;
                }
            }
            return res;
        }
    }
}