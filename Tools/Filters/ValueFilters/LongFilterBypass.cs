namespace NITHlibrary.Tools.Filters.ValueFilters
{
    public class LongFilterBypass : ILongFilter
    {
        private long val = 0;

        /// <summary>
        /// A filter which does... Nothing! Output = input.
        /// </summary>
        public LongFilterBypass()
        {

        }

        public void Push(long val)
        {
            this.val = val;
        }

        public long Pull()
        {
            return val;
        }
    }
}
