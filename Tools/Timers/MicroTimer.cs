/*
 * Project source: https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer
 * Modified to fit Threading requirements from .NET 8
 */


/*
 * Project source: https://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer
 * Modified to fit Threading requirements from .NET 8
 */

namespace NITHlibrary.Tools.Timers
{
    /// <summary>
    /// MicroTimer class
    /// </summary>
    public class MicroTimer
    {
        private CancellationTokenSource? _cancellationTokenSource = null;
        private long _ignoreEventIfLateBy = long.MaxValue;
        private Thread? _threadTimer = null;
        private long _timerIntervalInMicroSec = 0;

        public MicroTimer()
        { }

        public MicroTimer(long timerIntervalInMicroseconds)
        {
            Interval = timerIntervalInMicroseconds;
        }

        public delegate void MicroTimerElapsedEventHandler(object sender, MicroTimerEventArgs timerEventArgs);

        public event EventHandler<MicroTimerEventArgs> MicroTimerElapsed;

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
            get
            {
                return _threadTimer != null && _threadTimer.IsAlive;
            }
        }

        public long IgnoreEventIfLateBy
        {
            get { return Interlocked.Read(ref _ignoreEventIfLateBy); }
            set { Interlocked.Exchange(ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value); }
        }

        public long Interval
        {
            get { return Interlocked.Read(ref _timerIntervalInMicroSec); }
            set { Interlocked.Exchange(ref _timerIntervalInMicroSec, value); }
        }

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

        public void Start()
        {
            if (Enabled || Interval <= 0)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            ThreadStart threadStart = delegate ()
            {
                NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, _cancellationTokenSource.Token);
            };

            _threadTimer = new Thread(threadStart)
            {
                Priority = ThreadPriority.Highest
            };
            _threadTimer.Start();
        }

        public void Stop()
        {
            Abort();
        }

        public void StopAndWait()
        {
            StopAndWait(Timeout.Infinite);
        }

        public bool StopAndWait(int timeoutInMilliSec)
        {
            Abort();
            return !_threadTimer.IsAlive;
        }

        private void NotificationTimer(ref long timerIntervalInMicroSec, ref long ignoreEventIfLateBy, CancellationToken token)
        {
            int timerCount = 0;
            long nextNotification = 0;

            MicroStopwatch microStopwatch = new MicroStopwatch();
            microStopwatch.Start();

            while (!token.IsCancellationRequested)
            {
                long callbackFunctionExecutionTime = microStopwatch.ElapsedMicroseconds - nextNotification;

                long timerIntervalInMicroSecCurrent = Interlocked.Read(ref timerIntervalInMicroSec);
                long ignoreEventIfLateByCurrent = Interlocked.Read(ref ignoreEventIfLateBy);

                nextNotification += timerIntervalInMicroSecCurrent;
                timerCount++;
                long elapsedMicroseconds;
                while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds) < nextNotification)
                {
                    Thread.SpinWait(10);
                    if (token.IsCancellationRequested) break;
                }

                if (token.IsCancellationRequested) break;

                long timerLateBy = elapsedMicroseconds - nextNotification;

                if (timerLateBy >= ignoreEventIfLateByCurrent)
                {
                    continue;
                }

                MicroTimerEventArgs microTimerEventArgs = new MicroTimerEventArgs(timerCount, elapsedMicroseconds, timerLateBy, callbackFunctionExecutionTime);
                MicroTimerElapsed?.Invoke(this, microTimerEventArgs);
            }

            microStopwatch.Stop();
        }
    }
}