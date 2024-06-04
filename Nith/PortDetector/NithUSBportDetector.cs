using System.IO.Ports;
using System.Text;
using System.Timers;

namespace NITHlibrary.Nith.PortDetector
{
    public class NithUSBportDetector
    {
        private const int MAX_TRIALS = 5;
        private const string NITH_PATTERN = "$Nith";
        private const int TIMEOUT_MILLISECONDS = 1000;
        private bool isScanning = false;
        private List<string> portNames;
        private Dictionary<string, string> portsAndSensorNames;
        private Dictionary<SerialPort, int> portsAndTrials; // Serial port, number of trials

        public List<INithUSBportDetectorBehavior> Behaviors { get; set; } = new List<INithUSBportDetectorBehavior>();

        private NithPortDetectorInfo status;

        // Port name, sensor name
        private int scannedPorts = 0;

        private System.Timers.Timer timeOutTimer;

        public NithUSBportDetector()
        {
            portsAndTrials = new Dictionary<SerialPort, int>();
            portNames = new List<string>();
            portsAndSensorNames = new Dictionary<string, string>();
        }

        private void ListNithSensorsAndPorts()
        {
            Console.WriteLine("USB scan completed.");
            // Check if the dictionary is not null or empty
            if (portsAndSensorNames != null && portsAndSensorNames.Count > 0)
            {
                StringBuilder sb = new StringBuilder();



                // Iterate over the dictionary and append each key-value pair to the StringBuilder
                foreach (var pair in portsAndSensorNames)
                {
                    sb.AppendLine($"Port: {pair.Key}, Sensor Name: {pair.Value}");
                }

                // Display the contents using MessageBox
                Console.WriteLine("Detected NITH sensors and their USB ports:\n" + sb.ToString());
            }
            else
            {
                Console.WriteLine("No NITH sensor found on USB ports!");
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>False, if the scanning process is ongoing (blocked)</returns>
        public bool Scan()
        {
            // Block if already doing
            if (isScanning) { Console.WriteLine("Error: already scanning!"); return false; }
            isScanning = true;

            Console.WriteLine("Scanning USB ports in search of NITH sensors");

            // Notify Behaviors that scanning has started
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorInfo.Scanning, portsAndSensorNames);
            }

            // Close all ports for safety, and clear detected ports
            CloseAllPorts();
            ClearDetectedSensors();

            // Initialize scanned ports number to zero
            scannedPorts = 0;

            // List all port names
            portNames = SerialPort.GetPortNames().ToList();

            // Create and open a port for each portName
            foreach (var portName in portNames)
            {
                // Initialize the port
                SerialPort serialPort = new SerialPort();
                serialPort.BaudRate = 115200;
                serialPort.Parity = Parity.Even;
                serialPort.DataBits = 8;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.PortName = portName; // assign name
                serialPort.DataReceived += SerialPort_DataReceived;

                // Add the port to the list
                bool alreadyIn = false;
                foreach (var entry in portsAndTrials)
                {
                    if (entry.Key.PortName == serialPort.PortName)
                    {
                        alreadyIn = true;
                        break;
                    }
                }
                if (!alreadyIn)
                {
                    try
                    {
                        portsAndTrials.Add(serialPort, 0);
                    }
                    catch
                    {
                        FinishedWithError();
                    }
                }
            }

            // Small tweak just to avoid "collection modified" error
            var copy = new Dictionary<SerialPort, int>(portsAndTrials.Count);
            foreach (var entry in portsAndTrials)
            {
                copy.Add(entry.Key, entry.Value);
            }

            // Open all ports
            foreach (var entry in copy)
            {
                SerialPort port = entry.Key;
                try
                {
                    port.Open();
                    port.DiscardInBuffer(); // clear buffer
                }
                catch
                {
                    // if couldn't open, well, no problem. This port has been scanned, result negative
                    scannedPorts++;
                    port.Close();
                }
            }

            // Set scan timeout timer
            if (timeOutTimer != null) timeOutTimer.Dispose();
            timeOutTimer = new System.Timers.Timer();
            timeOutTimer.Interval = TIMEOUT_MILLISECONDS;
            timeOutTimer.Elapsed += Timer_Elapsed;
            timeOutTimer.Start();

            return true; // If everything went well, return true
        }

        private void FinishedWithError()
        {
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorInfo.Error, portsAndSensorNames);
            }
            isScanning = false;
        }

        private void ClearDetectedSensors()
        {
            portsAndSensorNames.Clear();
        }

        private void CloseAllPorts()
        {
            foreach (var entry in portsAndTrials)
            {
                entry.Key.Close();
                entry.Key.Dispose();
            }
        }

        private string GetPortsAndSensorsList()
        {
            // Initialize an empty StringBuilder
            StringBuilder sb = new StringBuilder();

            // Iterate through all key-value pairs in the dictionary
            foreach (var pair in portsAndSensorNames)
            {
                // Append the port and sensor name to the StringBuilder
                sb.AppendFormat("{0}: {1}", pair.Key, pair.Value);

                // Append a newline character to separate each entry
                // Remove this line if you want everything on a single line
                sb.AppendLine();
            }

            // Convert the StringBuilder to a single string
            return sb.ToString().TrimEnd(); // TrimEnd to remove the last newline character
        }

        private void FinishedWithSuccess()
        {
            // Stop timeout timer
            timeOutTimer.Stop();
            timeOutTimer.Dispose();

            // List ports
            ListNithSensorsAndPorts();

            // Send to Behaviors
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorInfo.Finished, portsAndSensorNames);
            }
            isScanning = false;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Typecast
            SerialPort port = sender as SerialPort;

            // Read
            string line = port.ReadLine();

            // Increment trials
            portsAndTrials[port]++;

            // Check...
            if (line.Contains(NITH_PATTERN))
            {
                try
                {
                    // Is it splittable?
                    string sensorName = line.Split('|')[0].Replace("$", "");

                    // SUCCESS!
                    // Add to results (only if not already in)
                    if (!portsAndSensorNames.ContainsKey(port.PortName))
                    {
                        portsAndSensorNames.Add(port.PortName, sensorName);
                    }

                    // Close and increment scanned ports
                    port.Close();
                    scannedPorts++;
                }
                catch
                {
                    // Do nothing. Wait next round.
                }
            }

            // If trial numbers expired, close.
            if (portsAndTrials[port] >= MAX_TRIALS)
            {
                port.Close();
                scannedPorts++;
            }

            // If process ended (scanned ports finished), it's done!
            if (scannedPorts == portsAndTrials.Count)
            {
                FinishedWithSuccess();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeOutTimer.Stop();
            CloseAllPorts();
            timeOutTimer.Dispose();

            if (isScanning)
                FinishedWithSuccess();
        }
    }
}