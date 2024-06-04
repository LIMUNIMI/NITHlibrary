namespace NITHlibrary.Tools.Filters.ValueFilters
{
    public interface IDoubleFilter
    {
        void Push(double value);
        double Pull();
    }
}
