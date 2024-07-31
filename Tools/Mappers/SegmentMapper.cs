namespace NITHlibrary.Tools.Mappers
{
    /// <summary>
    /// Provides functionality to map a value from one range to another.
    /// For example, this can be used to map sensor readings from a range of [-250; 250] to [0; 100].
    /// </summary>
    public class SegmentMapper
    {
        private double _baseSpan;
        private double _targetSpan;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentMapper"/> class with the specified ranges.
        /// </summary>
        /// <param name="baseMin">The minimum value of the base range.</param>
        /// <param name="baseMax">The maximum value of the base range.</param>
        /// <param name="targetMin">The minimum value of the target range.</param>
        /// <param name="targetMax">The maximum value of the target range.</param>
        /// <param name="cutOutOfRangeValues">Specifies whether to cut values that are out of the target range.</param>
        public SegmentMapper(double baseMin, double baseMax, double targetMin, double targetMax, bool cutOutOfRangeValues = false)
        {
            SetBaseRange(baseMin, baseMax);
            SetTargetRange(targetMin, targetMax);
            CutOutOfRangeValues = cutOutOfRangeValues;
        }

        /// <summary>
        /// Gets the maximum value of the base range.
        /// </summary>
        public double BaseMax { get; private set; }

        /// <summary>
        /// Gets the minimum value of the base range.
        /// </summary>
        public double BaseMin { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cut values that are out of the target range.
        /// </summary>
        public bool CutOutOfRangeValues { get; set; }

        /// <summary>
        /// Gets the maximum value of the target range.
        /// </summary>
        public double TargetMax { get; private set; }

        /// <summary>
        /// Gets the minimum value of the target range.
        /// </summary>
        public double TargetMin { get; private set; }

        /// <summary>
        /// Maps a value from the base range to the target range.
        /// </summary>
        /// <param name="value">The value to map from the base range.</param>
        /// <returns>The mapped value in the target range.</returns>
        public double Map(double value)
        {
            var ret = TargetMin + (value - BaseMin) / _baseSpan * _targetSpan;

            // Cut values which are out of range?
            if (CutOutOfRangeValues)
            {
                if (ret > TargetMax) ret = TargetMax;
                if (ret < TargetMin) ret = TargetMin;
            }
            return ret;
        }

        /// <summary>
        /// Sets the range of the base values.
        /// </summary>
        /// <param name="baseMin">The minimum value of the base range.</param>
        /// <param name="baseMax">The maximum value of the base range.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseMax"/> is not greater than <paramref name="baseMin"/>.</exception>
        public void SetBaseRange(double baseMin, double baseMax)
        {
            if (baseMax <= baseMin)
            {
                throw new ArgumentException("BaseMax must be greater than BaseMin.");
            }

            BaseMin = baseMin;
            BaseMax = baseMax;
            RecalculateBaseSpan();
        }

        /// <summary>
        /// Sets the range of the target values.
        /// </summary>
        /// <param name="targetMin">The minimum value of the target range.</param>
        /// <param name="targetMax">The maximum value of the target range.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetMax"/> is not greater than <paramref name="targetMin"/>.</exception>
        public void SetTargetRange(double targetMin, double targetMax)
        {
            if (targetMax <= targetMin)
            {
                throw new ArgumentException("TargetMax must be greater than TargetMin.");
            }

            TargetMin = targetMin;
            TargetMax = targetMax;
            RecalculateTargetSpan();
        }

        /// <summary>
        /// Recalculates the span of the base range.
        /// </summary>
        private void RecalculateBaseSpan()
        {
            _baseSpan = BaseMax - BaseMin;
        }

        /// <summary>
        /// Recalculates the span of the target range.
        /// </summary>
        private void RecalculateTargetSpan()
        {
            _targetSpan = TargetMax - TargetMin;
        }
    }
}