using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Behaviors
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
        protected int CounterDeC = 0; // Double eye close counter
        protected int CounterDeO = 0; // Double eye open counter
        protected int CounterLeC = 0; // Left eye close counter
        protected int CounterLeO = 0; // Left eye open counter
        protected int CounterReC = 0; // Right eye close counter
        protected int CounterReO = 0; // Right eye open counter

        // Thresholds for triggering events
        public int DcThresh { get; set; } = Defaultthresh;
        public int DoThresh { get; set; } = Defaultthresh;
        public int LcThresh { get; set; } = Defaultthresh;
        public int LoThresh { get; set; } = Defaultthresh;
        public int RcThresh { get; set; } = Defaultthresh;
        public int RoThresh { get; set; } = Defaultthresh;

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
                    CounterLeO = 0;
                    CounterLeC++;
                    CounterReO++;
                    CounterReC = 0;
                    CounterDeO = 0;
                    CounterDeC = 0;
                }
                else if (isLEopen && !isREopen) // Right eye is blinking
                {
                    CounterLeO++;
                    CounterLeC = 0;
                    CounterReO = 0;
                    CounterReC++;
                    CounterDeO = 0;
                    CounterDeC = 0;
                }
                else if (!isLEopen && !isREopen) // Double eye blinking
                {
                    CounterLeO = 0;
                    CounterLeC = 0;
                    CounterReO = 0;
                    CounterReC = 0;
                    CounterDeO = 0;
                    CounterDeC++;
                }
                else if (isLEopen && isREopen) // No blinking
                {
                    CounterLeO++;
                    CounterLeC = 0;
                    CounterReO++;
                    CounterReC = 0;
                    CounterDeO++;
                    CounterDeC = 0;
                }

                // Trigger events if thresholds are met
                if (CounterLeO == LoThresh)
                {
                    Event_leftOpen();
                }
                if (CounterLeC == LcThresh)
                {
                    Event_leftClose();
                }
                if (CounterReO == RoThresh)
                {
                    Event_rightOpen();
                }
                if (CounterReC == RcThresh)
                {
                    Event_rightClose();
                }
                if (CounterDeO == DoThresh)
                {
                    Event_doubleOpen();
                }
                if (CounterDeC == DcThresh)
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
