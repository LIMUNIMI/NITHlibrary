namespace NITHlibrary.Tools.Mappers
{
    public class VelocityExtractor1D
    {
        public double InstantSpeed { get; set; } = 0;
        private double PastValue { get; set; }
        private bool first = true;
        public int Direction { get; set; } = 0;
        private int NValues { get; set; }
        public double MultiplyFactor { get; set; }

        public VelocityExtractor1D(double multiplyFactor = 1f)
        {
            MultiplyFactor = multiplyFactor;
        }

        public void Push(double value)
        {
            if (first)
            {
                PastValue = value;
                first = false;
            }

            InstantSpeed = (value - PastValue) * MultiplyFactor;
            PastValue = value;

            Direction = Math.Sign(InstantSpeed);

            InstantSpeed = Math.Abs(InstantSpeed);
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
