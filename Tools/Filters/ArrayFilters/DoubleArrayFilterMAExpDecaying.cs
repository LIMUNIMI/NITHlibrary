namespace NITHlibrary.Tools.Filters.ArrayFilters
{
    public class DoubleArrayFilterMAExpDecaying : IDoubleArrayFilter
    {
        private double[] arrI;
        private double[] arrIplusOne;
        private float alpha;

        /// <summary>
        /// The classic implementation of an exponentially decaying moving average filter. 
        /// </summary>
        /// <param name="alpha">Indicates the speed of decreasing priority of the old values.</param>
        public DoubleArrayFilterMAExpDecaying(float alpha)
        {
            this.alpha = alpha;
        }

        public void Push(double[] input)
        {
            if (arrIplusOne == null) { arrIplusOne = input; }

            arrI = arrIplusOne;
            for (int i = 0; i < arrI.Length; i++)
            {
                arrIplusOne[i] = alpha * input[i] + (1 - alpha) * arrI[i];
            }
        }

        public double[] Pull()
        {
            return arrIplusOne;
        }
    }
}
