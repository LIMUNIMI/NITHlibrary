namespace NITHlibrary.Tools.Timers
{
    /// <summary>
    /// Represents a high-precision timer that executes a task at defined intervals measured in microseconds.
    /// Inspired by the MicroTimer library (https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer).
    /// </summary>
    public class MicroTimer
    {
        private CancellationTokenSource? _cancellationTokenSource = null;
        private long _ignoreEventIfLateBy = long.MaxValue;
        private Thread? _threadTimer = null;
        private long _timerIntervalInMicroSec = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroTimer"/> class.
        /// </summary>
        public MicroTimer()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicroTimer"/> class with a specified interval.
        /// </summary>
        /// <param name="timerIntervalInMicroseconds">The interval in microseconds between timer events.</param>
        public MicroTimer(long timerIntervalInMicroseconds)
        {
            Interval = timerIntervalInMicroseconds;
        }

        /// <summary>
        /// Represents the method that handles the <see cref="MicroTimerElapsed"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="timerEventArgs">An object that contains the event data.</param>
        public delegate void MicroTimerElapsedEventHandler(object sender, MicroTimerEventArgs timerEventArgs);

        /// <summary>
        /// Occurs when the timer interval has elapsed.
        /// </summary>
        public event EventHandler<MicroTimerEventArgs> MicroTimerElapsed;

        /// <summary>
        /// Gets or sets a value indicating whether the timer is running.
        /// </summary>
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
            get => _threadTimer != null && _threadTimer.IsAlive;
        }

        /// <summary>
        /// Gets or sets the amount of time to ignore an event if it's late by this amount of time.
        /// </summary>
        public long IgnoreEventIfLateBy
        {
            get => Interlocked.Read(ref _ignoreEventIfLateBy);
            set => Interlocked.Exchange(ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value);
        }

        /// <summary>
        /// Gets or sets the interval between timer events in microseconds.
        /// </summary>
        public long Interval
        {
            get => Interlocked.Read(ref _timerIntervalInMicroSec);
            set => Interlocked.Exchange(ref _timerIntervalInMicroSec, value);
        }

        /// <summary>
        /// Aborts the timer, stopping its thread and disposing of resources.
        /// </summary>
        private void Abort()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();

                if (Enabled)
                {
                    _threadTimer.Join(); // Wait for the thread to terminate gracefully
                }

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if (Enabled || Interval <= 0)
            {
                return;
            }

            _cancellationTokenSource = new();

            ThreadStart threadStart = delegate ()
            {
                NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, _cancellationTokenSource.Token);
            };

            _threadTimer = new(threadStart)
            {
                Priority = ThreadPriority.Highest
            };
            _threadTimer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            Abort();
        }

        /// <summary>
        /// Stops the timer and waits indefinitely for it to stop.
        /// </summary>
        public void StopAndWait()
        {
            StopAndWait(Timeout.Infinite);
        }

        /// <summary>
        /// Stops the timer and waits for the specified amount of time for it to stop.
        /// </summary>
        /// <param name="timeoutInMilliSec">The amount of time in milliseconds to wait for the timer to stop.</param>
        /// <returns>True if the timer thread has stopped; otherwise, false.</returns>
        public bool StopAndWait(int timeoutInMilliSec)
        {
            Abort();
            return !_threadTimer.IsAlive;
        }

        /// <summary>
        /// Handles the notification logic for the timer.
        /// </summary>
        /// <param name="timerIntervalInMicroSec">The interval in microseconds between timer events.</param>
        /// <param name="ignoreEventIfLateBy">The amount of time to ignore an event if it's late by this amount of time.</param>
        /// <param name="token">The cancellation token used to stop the timer.</param>
        private void NotificationTimer(ref long timerIntervalInMicroSec, ref long ignoreEventIfLateBy, CancellationToken token)
        {
            var timerCount = 0;
            long nextNotification = 0;

            MicroStopwatch microStopwatch = new();
            microStopwatch.Start();

            while (!token.IsCancellationRequested)
            {
                var callbackFunctionExecutionTime = microStopwatch.ElapsedMicroseconds - nextNotification;

                var timerIntervalInMicroSecCurrent = Interlocked.Read(ref timerIntervalInMicroSec);
                var ignoreEventIfLateByCurrent = Interlocked.Read(ref ignoreEventIfLateBy);

                nextNotification += timerIntervalInMicroSecCurrent;
                timerCount++;
                long elapsedMicroseconds;
                while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds) < nextNotification)
                {
                    Thread.SpinWait(10);
                    if (token.IsCancellationRequested) break;
                }

                if (token.IsCancellationRequested) break;

                var timerLateBy = elapsedMicroseconds - nextNotification;

                if (timerLateBy >= ignoreEventIfLateByCurrent)
                {
                    continue;
                }

                MicroTimerEventArgs microTimerEventArgs = new(timerCount, elapsedMicroseconds, timerLateBy, callbackFunctionExecutionTime);
                MicroTimerElapsed?.Invoke(this, microTimerEventArgs);
            }

            microStopwatch.Stop();
        }
    }
}