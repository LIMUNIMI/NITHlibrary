using System.Diagnostics;

namespace NITHlibrary.Tools.Logging
{
    public static class TraceAdder
    {
        ///<summary>
        /// Adds trace information, error and warning messages to the trace logs.
        ///</summary>
        public static void AddTrace()
        {
            Trace.TraceInformation("Trace Information");
            Trace.TraceError("Trace Error");
            Trace.TraceWarning("Trace Warning");
            Trace.Listeners.Add(new TextWriterTraceListener("Error.log"));
        }
    }
}
