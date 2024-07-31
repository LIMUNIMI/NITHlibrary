namespace NITHlibrary.Tools.Filters.ArrayFilters
{
    /// <summary>
    /// A filter that applies an exponentially decaying moving average to a double array, smoothing every component temporally.
    /// </summary>
    public class DoubleArrayFilterMAExpDecaying : IDoubleArrayFilter
    {
        private double[] _arrI;
        private double[] _arrIplusOne;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleArrayFilterMAExpDecaying"/> class with a given smoothing factor.
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.</param>
        public DoubleArrayFilterMAExpDecaying(float alpha)
        {
            this.Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the smoothing factor that dictates the weight of the current input.
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// Retrieves the filtered output array.
        /// </summary>
        /// <returns>The filtered double array.</returns>
        public double[] Pull()
        {
            return _arrIplusOne;
        }

        /// <summary>
        /// Feeds a new input array to the filter and computes the exponentially decaying moving average.
        /// </summary>
        /// <param name="input">The input array to be filtered.</param>
        public void Push(double[] input)
        {
            _arrIplusOne ??= input;

            _arrI = _arrIplusOne;
            for (var i = 0; i < _arrI.Length; i++)
            {
                _arrIplusOne[i] = Alpha * input[i] + (1 - Alpha) * _arrI[i];
            }
        }
    }
}