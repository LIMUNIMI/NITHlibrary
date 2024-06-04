namespace NITHlibrary.Tools.Filters.ValueFilters
{
    public class LongFilterMAExpDecaying : ILongFilter
    {
        private double valI = 0;
        private double valIplusOne = 0;
        private float alpha;

        /// <summary>
        /// The classic implementation of an exponentially decaying moving average filter. 
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.</param>
        public LongFilterMAExpDecaying(float alpha)
        {
            this.alpha = alpha;
        }

        public void Push(long val)
        {
            valI = valIplusOne;
            valIplusOne = (long)(alpha * (double)val) + (1 - alpha) * valI;
        }

        public long Pull()
        {
            return (long)valIplusOne;
        }
    }
}
