namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// An interface which defines a NITH error behavior.
    /// </summary>
    public interface INithErrorBehavior
    {
        /// <summary>
        /// Defines how to handle an error.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>True if the error is handled, false otherwise.</returns>
        bool HandleError(NithErrors error);
    }
}
