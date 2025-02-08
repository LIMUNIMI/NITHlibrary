using NITHlibrary.Tools.Ports;

namespace NITHlibrary.Nith.PortDetector
{
    /// <summary>
    /// A behavior for the <see cref="NithUSBportDetector"/> which will connect to a NITH sensor on scan finished.
    /// You can specify which sensor you want to connect to; 
    /// otherwise, it will connect to the sensor with the lowest number of port.
    /// </summary>
    public class BUSBreceiver_ConnectToPort : INithUSBportDetectorBehavior
    {
        private readonly USBreceiver _usbReceiver;

        /// <summary>
        /// The sensor name to connect to. 
        /// If empty, it will connect to the sensor with the lowest port number.
        /// </summary>
        private string RequiredSensor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BUSBreceiver_ConnectToPort"/> class.
        /// </summary>
        /// <param name="usbReceiver">The <see cref="USBreceiver"/> which will handle the connection.</param>
        /// <param name="requiredSensor">Optional sensor name to connect to. If not specified, connects to the sensor with the lowest port number.</param>
        public BUSBreceiver_ConnectToPort(USBreceiver usbReceiver, string requiredSensor = "")
        {
            this.RequiredSensor = requiredSensor;
            this._usbReceiver = usbReceiver;
        }

        /// <summary>
        /// Connects to the specified or lowest number port sensor when scanning is finished.
        /// </summary>
        /// <param name="status">Status of the port detection process.</param>
        /// <param name="portsAndSensors">A dictionary containing port numbers and corresponding sensor names.</param>
        public void ProcessInformation(NithPortDetectorStatus status, Dictionary<string, string> portsAndSensors)
        {
            if (portsAndSensors.Count > 0 && status == NithPortDetectorStatus.Finished)
            {
                if (RequiredSensor == string.Empty)
                {
                    var portMin = 0;
                    foreach (KeyValuePair<string, string> kvp in portsAndSensors)
                    {
                        if (int.Parse(kvp.Key) < portMin)
                        {
                            portMin = int.Parse(kvp.Key);
                        }
                    }
                    _usbReceiver.Connect(portMin);
                }
                else // requiredSensor not empty
                {
                    foreach (KeyValuePair<string, string> kvp in portsAndSensors)
                    {
                        if (kvp.Value.Contains(RequiredSensor))
                        {
                            string digits = new(kvp.Key.Where(char.IsDigit).ToArray());
                            // Parse the extracted digits as an integer
                            var number = int.Parse(digits);
                            Console.WriteLine("Connecting to port " + number.ToString() + "\nSensor: " + RequiredSensor);

                            _usbReceiver.Connect(number);
                            Console.WriteLine("Connected!");
                            return;
                        }
                    }
                }
            }
        }
    }
}