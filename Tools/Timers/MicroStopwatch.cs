namespace NITHlibrary.Tools.Timers
{
    /// <summary>
    /// Represents a high-resolution stopwatch capable of measuring elapsed time in microseconds.
    /// Inherits from the System.Diagnostics.Stopwatch class.
    /// Inspired by the MicroTimer library (https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer).
    /// </summary>
    public class MicroStopwatch : System.Diagnostics.Stopwatch
    {
        /// <summary>
        /// The number of microseconds per tick of the stopwatch's frequency.
        /// </summary>
        private readonly double _microSecPerTick =
            1000000D / Frequency;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroStopwatch"/> class.
        /// Throws an exception if the high-resolution performance counter is not available on the system.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the high-resolution performance counter is not available on the system.</exception>
        public MicroStopwatch()
        {
            if (!IsHighResolution)
            {
                throw new InvalidOperationException("On this system the high-resolution performance counter is not available");
            }
        }

        /// <summary>
        /// Gets the elapsed time measured by the current instance, in microseconds.
        /// </summary>
        public long ElapsedMicroseconds => (long)(ElapsedTicks * _microSecPerTick);
    }
}