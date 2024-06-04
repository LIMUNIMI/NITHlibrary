namespace NITHlibrary.Tools.Mappers
{
    public class SegmentMapper
    {
        private double baseSpan;
        private double targetSpan;

        public SegmentMapper(double baseMin, double baseMax, double targetMin, double targetMax, bool cutOutOfRangeValues = false)
        {
            SetBaseRange(baseMin, baseMax);
            SetTargetRange(targetMin, targetMax);
            CutOutOfRangeValues = cutOutOfRangeValues;
        }

        public double BaseMax { get; private set; }
        public double BaseMin { get; private set; }
        public bool CutOutOfRangeValues { get; set; }
        public double TargetMax { get; private set; }
        public double TargetMin { get; private set; }

        public double Map(double value)
        {
            double ret = TargetMin + (value - BaseMin) / baseSpan * targetSpan;

            // Cut values which are out of range?
            if (CutOutOfRangeValues)
            {
                if (ret > TargetMax) ret = TargetMax;
                if (ret < TargetMin) ret = TargetMin;
            }
            return ret;
        }

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

        private void RecalculateBaseSpan()
        {
            baseSpan = BaseMax - BaseMin;
        }

        private void RecalculateTargetSpan()
        {
            targetSpan = TargetMax - TargetMin;
        }
    }
}