namespace NITHlibrary.Tools.Timers
{
    /// <summary>
    /// Represents the event arguments for the MicroTimer event.
    /// Provides information about the timer event such as the timer count, 
    /// elapsed time, timer delay, and execution time of the callback function.
    /// Inspired by the MicroTimer library (https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer).
    /// </summary>
    public class MicroTimerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicroTimerEventArgs"/> class.
        /// </summary>
        /// <param name="timerCount">The number of times the timed event (callback function) has executed.</param>
        /// <param name="elapsedMicroseconds">The time in microseconds since the timer started when the timed event was called.</param>
        /// <param name="timerLateBy">How late the timer was compared to when it should have been called.</param>
        /// <param name="callbackFunctionExecutionTime">The time it took to execute the previous call to the callback function (OnTimedEvent).</param>
        public MicroTimerEventArgs(
            int timerCount,
            long elapsedMicroseconds,
            long timerLateBy,
            long callbackFunctionExecutionTime)
        {
            TimerCount = timerCount;
            ElapsedMicroseconds = elapsedMicroseconds;
            TimerLateBy = timerLateBy;
            CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
        }

        /// <summary>
        /// Gets the time it took to execute the previous call to the callback function (OnTimedEvent).
        /// </summary>
        public long CallbackFunctionExecutionTime { get; }

        /// <summary>
        /// Gets the time in microseconds since the timer started when the timed event was called.
        /// </summary>
        public long ElapsedMicroseconds { get; }

        /// <summary>
        /// Gets the number of times the timed event (callback function) has executed.
        /// </summary>
        public int TimerCount { get; }

        /// <summary>
        /// Gets how late the timer was compared to when it should have been called.
        /// </summary>
        public long TimerLateBy { get; }
    }
}