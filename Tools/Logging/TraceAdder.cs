using System.Diagnostics;

namespace NITHlibrary.Tools.Logging
{
    /// <summary>
    /// Provides methods to add trace information, errors, and warnings to the trace logs.
    /// </summary>
    public static class TraceAdder
    {
        /// <summary>
        /// Adds trace information, error and warning messages to the trace logs and attaches a 
        /// TextWriterTraceListener to log errors to a file named "Error.log".
        /// </summary>
        public static void AddTrace()
        {
            Trace.TraceInformation("Trace Information");
            Trace.TraceError("Trace Error");
            Trace.TraceWarning("Trace Warning");
            Trace.Listeners.Add(new TextWriterTraceListener("Error.log"));
        }
    }
}