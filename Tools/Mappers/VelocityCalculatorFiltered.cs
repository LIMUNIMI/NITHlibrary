namespace NITHlibrary.Tools.Mappers
{
    /// <summary>
    /// The VelocityCalculatorFiltered class provides functionality to compute a filtered velocity and direction
    /// from a series of pushed values. It averages the input values using a specified multiply factor
    /// and tracks the direction of the velocity.
    /// </summary>
    public class VelocityCalculatorFiltered
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VelocityCalculatorFiltered"/> class.
        /// </summary>
        /// <param name="nValues">The number of values to store for calculation.</param>
        /// <param name="multiplyFactor">The factor used to multiply the average deviation. Default is 1.0.</param>
        public VelocityCalculatorFiltered(int nValues, double multiplyFactor = 1f)
        {
            NValues = nValues;
            MultiplyFactor = multiplyFactor;
        }

        /// <summary>
        /// Gets the direction of the instantaneous speed.
        /// </summary>
        public int Direction { get; private set; } = 0;

        /// <summary>
        /// Gets instantaneous speed.
        /// </summary>
        public double InstantSpeed { get; private set; } = 0;

        /// <summary>
        /// Gets or sets the multiplier factor applied to the calculated average deviation.
        /// </summary>
        public double MultiplyFactor { get; set; }

        /// <summary>
        /// Gets or sets the number of values to be stored for averaging.
        /// </summary>
        public int NValues { get; set; }

        /// <summary>
        /// Stores the list of values used to calculate the filtered velocity.
        /// </summary>
        private List<double> ValuesMemory { get; set; } = [];

        /// <summary>
        /// Pulls the last calculated direction.
        /// </summary>
        /// <returns>The last calculated direction.</returns>
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
        /// Pushes a new value to the memory and updates the instantaneous speed and direction.
        /// </summary>
        /// <param name="value">The new value to be pushed.</param>
        public void Push(double value)
        {
            double avg = 0;
            var nvals = GetRecursiveSum(ValuesMemory.Count);

            for (var i = 0; i < ValuesMemory.Count; i++)
            {
                avg += ValuesMemory[i] * (i + 1);
            }
            avg = avg / nvals;

            if (ValuesMemory.Count == NValues)
            {
                ValuesMemory.RemoveAt(0);
            }

            ValuesMemory.Add(value);

            avg = (value - avg) * MultiplyFactor;

            Direction = Math.Sign(avg);

            avg = Math.Abs(avg);

            InstantSpeed = avg;
        }

        /// <summary>
        /// Calculates the recursive sum of integers up to the specified number.
        /// </summary>
        /// <param name="n">The number up to which to calculate the sum.</param>
        /// <returns>The recursive sum of integers up to the specified number.</returns>
        private static int GetRecursiveSum(int n)
        {
            var sum = 0;
            for (var i = 1; i <= n; i++)
            {
                sum += i;
            }
            return sum;
        }
    }
}