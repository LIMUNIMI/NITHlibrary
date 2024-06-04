namespace NITHlibrary.Tools.Mappers
{
    internal class VelocityExtractorFiltered1D
    {
        private double InstantSpeed { get; set; } = 0;
        public int Direction { get; set; } = 0;
        private int NValues { get; set; }
        public double MultiplyFactor { get; set; }
        private List<double> ValuesMemory { get; set; } = new List<double>();


        public VelocityExtractorFiltered1D(int nValues, double multiplyFactor = 1f)
        {
            NValues = nValues;
            MultiplyFactor = multiplyFactor;
        }

        public void Push(double value)
        {
            double avg = 0;
            int nvals = getRecursiveSum(ValuesMemory.Count);

            for (int i = 0; i < ValuesMemory.Count; i++)
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

        public double Pull()
        {
            return InstantSpeed;
        }

        private int getRecursiveSum(int n)
        {
            int sum = 0;
            for (int i = 1; i <= n; i++)
            {
                sum += i;
            }
            return sum;
        }
    }
}
