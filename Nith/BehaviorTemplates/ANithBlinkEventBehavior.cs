using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.BehaviorTemplates
{
    /// <summary>
    /// Defines a blink event behavior. This is a metabehavior which defines a more comfortable way to trigger events on eye closure/aperture events.
    /// Thresholds are defined as number of closure/aperture frames, and are tweakable.
    /// </summary>
    public abstract class ANithBlinkEventBehavior : INithSensorBehavior
    {
        public const int Defaultthresh = 5;

        private readonly List<NithParameters> _requiredArguments =
        [
            NithParameters.eyeLeft_isOpen,
            NithParameters.eyeRight_isOpen
        ];

        // Counters for tracking the state of the eyes
        protected int CounterDEC = 0; // Double eye close counter
        protected int CounterDEO = 0; // Double eye open counter
        protected int CounterLEC = 0; // Left eye close counter
        protected int CounterLEO = 0; // Left eye open counter
        protected int CounterREC = 0; // Right eye close counter
        protected int CounterREO = 0; // Right eye open counter

        // Thresholds for triggering events
        public int DCThresh { get; set; } = Defaultthresh;
        public int DOThresh { get; set; } = Defaultthresh;
        public int LCThresh { get; set; } = Defaultthresh;
        public int LOThresh { get; set; } = Defaultthresh;
        public int RCThresh { get; set; } = Defaultthresh;
        public int ROThresh { get; set; } = Defaultthresh;

        /// <summary>
        /// Handles incoming sensor data and updates the blink counters accordingly. Triggers events if thresholds are met.
        /// </summary>
        /// <param name="nithData">The sensor data received.</param>
        public void HandleData(NithSensorData nithData)
        {
            if (nithData.ContainsParameters(_requiredArguments))
            {
                var isLEopen = bool.Parse(nithData.GetParameterValue(NithParameters.eyeLeft_isOpen).Value.Base);
                var isREopen = bool.Parse(nithData.GetParameterValue(NithParameters.eyeRight_isOpen).Value.Base);

                // Update counters based on the state of the eyes
                if (!isLEopen && isREopen) // Left eye is blinking
                {
                    CounterLEO = 0;
                    CounterLEC++;
                    CounterREO++;
                    CounterREC = 0;
                    CounterDEO = 0;
                    CounterDEC = 0;
                }
                else if (isLEopen && !isREopen) // Right eye is blinking
                {
                    CounterLEO++;
                    CounterLEC = 0;
                    CounterREO = 0;
                    CounterREC++;
                    CounterDEO = 0;
                    CounterDEC = 0;
                }
                else if (!isLEopen && !isREopen) // Double eye blinking
                {
                    CounterLEO = 0;
                    CounterLEC = 0;
                    CounterREO = 0;
                    CounterREC = 0;
                    CounterDEO = 0;
                    CounterDEC++;
                }
                else if (isLEopen && isREopen) // No blinking
                {
                    CounterLEO++;
                    CounterLEC = 0;
                    CounterREO++;
                    CounterREC = 0;
                    CounterDEO++;
                    CounterDEC = 0;
                }

                // Trigger events if thresholds are met
                if (CounterLEO == LOThresh)
                {
                    Event_leftOpen();
                }
                if (CounterLEC == LCThresh)
                {
                    Event_leftClose();
                }
                if (CounterREO == ROThresh)
                {
                    Event_rightOpen();
                }
                if (CounterREC == RCThresh)
                {
                    Event_rightClose();
                }
                if (CounterDEO == DOThresh)
                {
                    Event_doubleOpen();
                }
                if (CounterDEC == DCThresh)
                {
                    Event_doubleClose();
                }
            }
        }

        /// <summary>
        /// Event triggered when both eyes are closed for the defined threshold.
        /// </summary>
        protected abstract void Event_doubleClose();

        /// <summary>
        /// Event triggered when both eyes are open for the defined threshold.
        /// </summary>
        protected abstract void Event_doubleOpen();

        /// <summary>
        /// Event triggered when the left eye is closed for the defined threshold.
        /// </summary>
        protected abstract void Event_leftClose();

        /// <summary>
        /// Event triggered when the left eye is open for the defined threshold.
        /// </summary>
        protected abstract void Event_leftOpen();

        /// <summary>
        /// Event triggered when the right eye is closed for the defined threshold.
        /// </summary>
        protected abstract void Event_rightClose();

        /// <summary>
        /// Event triggered when the right eye is open for the defined threshold.
        /// </summary>
        protected abstract void Event_rightOpen();
    }
}
