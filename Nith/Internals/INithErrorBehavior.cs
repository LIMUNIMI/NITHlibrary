namespace NITHlibrary.Nith.Internals
{
    public interface INithErrorBehavior
    {
        bool HandleError(NithErrors error);
    }
}
