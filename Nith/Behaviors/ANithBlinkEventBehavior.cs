using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Behaviors
{
    /// <summary>
    /// Defines a blink event behavior. This is a metabehavior which defines a more comfortable way to trigger events on eye closure/aperture events.
    /// Thresholds are defined as number of closure/aperture frames, and are tweakable.
    /// </summary>
    public abstract class ANithBlinkEventBehavior : INithSensorBehavior
    {
        public const int DEFAULTTHRESH = 5;

        private readonly List<NithParameters> requiredArguments = new()
        {
            NithParameters.eyeLeft_isOpen, NithParameters.eyeRight_isOpen
        };

        protected int counter_DE_C = 0;
        protected int counter_DE_O = 0;
        protected int counter_LE_C = 0;
        protected int counter_LE_O = 0;
        protected int counter_RE_C = 0;
        protected int counter_RE_O = 0;

        // Thresholds
        public int DCThresh { get; set; } = DEFAULTTHRESH;
        public int DOThresh { get; set; } = DEFAULTTHRESH;
        public int LCThresh { get; set; } = DEFAULTTHRESH;
        public int LOThresh { get; set; } = DEFAULTTHRESH;
        public int RCThresh { get; set; } = DEFAULTTHRESH;
        public int ROThresh { get; set; } = DEFAULTTHRESH;

        public void HandleData(NithSensorData nithData)
        {
            if (nithData.ContainsParameters(requiredArguments))
            {
                bool isLEopen = bool.Parse(nithData.GetParameter(NithParameters.eyeLeft_isOpen).Value.Base);
                bool isREopen = bool.Parse(nithData.GetParameter(NithParameters.eyeRight_isOpen).Value.Base);

                // Update counters
                if (isLEopen == false && isREopen == true) // Left eye is blinking
                {
                    counter_LE_O = 0;
                    counter_LE_C++;
                    counter_RE_O++;
                    counter_RE_C = 0;
                    counter_DE_O = 0;
                    counter_DE_C = 0;
                }
                else if (isLEopen == true && isREopen == false) // Right eye is blinking
                {
                    counter_LE_O++;
                    counter_LE_C = 0;
                    counter_RE_O = 0;
                    counter_RE_C++;
                    counter_DE_O = 0;
                    counter_DE_C = 0;
                }
                else if (isLEopen == false && isREopen == false) // Double eye blinking
                {
                    counter_LE_O = 0;
                    counter_LE_C = 0;
                    counter_RE_O = 0;
                    counter_RE_C = 0;
                    counter_DE_O = 0;
                    counter_DE_C++;
                }
                else if (isLEopen == true && isREopen == true) // No blinking
                {
                    counter_LE_O++;
                    counter_LE_C = 0;
                    counter_RE_O++;
                    counter_RE_C = 0;
                    counter_DE_O++;
                    counter_DE_C = 0;
                }

                // If thresholds are met, send to events
                if (counter_LE_O == LOThresh)
                {
                    Event_leftOpen();
                }
                if (counter_LE_C == LCThresh)
                {
                    Event_leftClose();
                }
                if (counter_RE_O == ROThresh)
                {
                    Event_rightOpen();
                }
                if (counter_RE_C == RCThresh)
                {
                    Event_rightClose();
                }
                if (counter_DE_O == DOThresh)
                {
                    Event_doubleOpen();
                }
                if (counter_DE_C == DCThresh)
                {
                    Event_doubleClose();
                }
            }
        }

        protected abstract void Event_doubleClose();

        protected abstract void Event_doubleOpen();

        protected abstract void Event_leftClose();

        protected abstract void Event_leftOpen();

        protected abstract void Event_rightClose();

        protected abstract void Event_rightOpen();
    }
}