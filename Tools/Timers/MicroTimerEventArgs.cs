namespace NITHlibrary.Tools.Timers
{
    /// <summary>
    /// MicroTimer Event Argument class
    /// </summary>
    public class MicroTimerEventArgs : EventArgs
    {
        public MicroTimerEventArgs(int timerCount,
                                           long elapsedMicroseconds,
                                           long timerLateBy,
                                           long callbackFunctionExecutionTime)
        {
            TimerCount = timerCount;
            ElapsedMicroseconds = elapsedMicroseconds;
            TimerLateBy = timerLateBy;
            CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
        }

        // Time it took to execute previous call to callback function (OnTimedEvent)
        public long CallbackFunctionExecutionTime { get; }

        // Time when timed event was called since timer started
        public long ElapsedMicroseconds { get; }

        // Simple counter, number times timed event (callback function) executed
        public int TimerCount { get; }

        // How late the timer was compared to when it should have been called
        public long TimerLateBy { get; }
    }
}