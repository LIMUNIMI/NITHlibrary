namespace NITHlibrary.Tools.Mappers
{
    /// <summary>
    /// Extracts and processes velocity data based on input values.
    /// This implementation only uses the last value and compares it to the previous one.
    /// </summary>
    public class VelocityCalculatorBasic
    {
        private bool _isFirst = true;
        private double _lastValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityCalculatorBasic"/> class.
        /// </summary>
        /// <param name="multiplyFactor">The factor to multiply the speed by. Default is 1.</param>
        public VelocityCalculatorBasic(double multiplyFactor = 1)
        {
            MultiplyFactor = multiplyFactor;
        }

        /// <summary>
        /// Gets or sets the factor by which the velocity is multiplied.
        /// </summary>
        public double MultiplyFactor { get; set; }

        /// <summary>
        /// Gets the direction of the velocity: -1 for negative, 1 for positive, 0 for no change.
        /// </summary>
        public int Direction { get; private set; }

        /// <summary>
        /// Gets the instant speed calculated from the velocity change.
        /// </summary>
        public double InstantSpeed { get; private set; }

        /// <summary>
        /// Pulls the direction of the last calculated velocity.
        /// </summary>
        /// <returns></returns>
        public double PullDirection()
        {
            return Direction;
        }

        /// <summary>
        /// Pulls the last calculated instantaneous speed.
        /// </summary>
        /// <returns>The last calculated instantaneous speed.</returns>
        public double PullInstantSpeed()
        {
            return InstantSpeed;
        }

        /// <summary>
        /// Updates the velocity extractor with a new value.
        /// </summary>
        /// <param name="value">The new value for processing velocity.</param>
        public void Push(double value)
        {
            if (_isFirst)
            {
                _lastValue = value;
                _isFirst = false;
            }
            else
            {
                var delta = value - _lastValue;
                InstantSpeed = Math.Abs(delta * MultiplyFactor);
                Direction = Math.Sign(delta);
                _lastValue = value;
            }
        }
    }
}