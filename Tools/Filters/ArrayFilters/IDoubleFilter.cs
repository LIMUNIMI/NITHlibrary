namespace NITHlibrary.Tools.Filters.ArrayFilters
{
    public interface IDoubleArrayFilter
    {
        void Push(double[] value);
        double[] Pull();
    }
}
