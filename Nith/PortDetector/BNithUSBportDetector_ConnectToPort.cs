using NITHlibrary.Tools.Ports;

namespace NITHlibrary.Nith.PortDetector
{
    /// <summary>
    /// Connects to a USB port on scan finished. You can specify which sensor you want to connect; otherwise, it will just connect to sensor with the lowest number of port
    /// </summary>
    public class BNithUSBportDetector_ConnectToPort : INithUSBportDetectorBehavior
    {
        private USBreceiver portManager;
        private string requiredSensor;

        public BNithUSBportDetector_ConnectToPort(USBreceiver portManager, string requiredSensor = "")
        {
            this.requiredSensor = requiredSensor;
            this.portManager = portManager;
        }

        public void ProcessInformation(NithPortDetectorInfo status, Dictionary<string, string> portsAndSensors)
        {
            if (portsAndSensors.Count > 0 && status == NithPortDetectorInfo.Finished)
            {
                if (requiredSensor == string.Empty)
                {
                    int portMin = 0;
                    foreach (KeyValuePair<string, string> kvp in portsAndSensors)
                    {
                        if (int.Parse(kvp.Key) < portMin)
                        {
                            portMin = int.Parse(kvp.Key);
                        }
                    }
                    portManager.Connect(portMin);
                }
                else // requiredSensor not empty
                {
                    foreach (KeyValuePair<string, string> kvp in portsAndSensors)
                    {
                        if (kvp.Value.Contains(requiredSensor))
                        {
                            string digits = new string(kvp.Key.Where(char.IsDigit).ToArray());
                            // Parse the extracted digits as an integer
                            int number = int.Parse(digits);
                            Console.WriteLine("Connecting to port " + number.ToString() + "\nSensor: " + requiredSensor);

                            portManager.Connect(number);
                            Console.WriteLine("Connected!");
                            return;
                        }
                    }
                }
            }
        }
    }
}