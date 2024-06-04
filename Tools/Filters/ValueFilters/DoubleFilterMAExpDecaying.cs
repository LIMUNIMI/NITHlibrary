namespace NITHlibrary.Tools.Filters.ValueFilters
{
    public class DoubleFilterMAExpDecaying : IDoubleFilter
    {
        private double valI = 0f;
        private double valIplusOne = 0f;
        private float alpha;

        /// <summary>
        /// The classic implementation of an exponentially decaying moving average filter. 
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.</param>
        public DoubleFilterMAExpDecaying(float alpha)
        {
            this.alpha = alpha;
        }

        public void Push(double val)
        {
            valI = valIplusOne;
            valIplusOne = alpha * val + (1 - alpha) * valI;
        }

        public double Pull()
        {
            return valIplusOne;
        }
    }
}
