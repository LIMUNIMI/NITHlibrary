namespace NITHlibrary.Tools.Filters.ValueFilters
{
    public class DoubleFilterBypass : IDoubleFilter
    {
        private double val = 0;
        /// <summary>
        /// A filter which does... Nothing! Output = input.
        /// </summary>
        public DoubleFilterBypass()
        {

        }

        public void Push(double val)
        {
            this.val = val;
        }

        public double Pull()
        {
            return val;
        }
    }
}
