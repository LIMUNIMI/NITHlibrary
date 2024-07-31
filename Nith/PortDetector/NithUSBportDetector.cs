using System.IO.Ports;
using System.Text;
using System.Timers;

namespace NITHlibrary.Nith.PortDetector
{
    /// <summary>
    /// An utility class to detect NITH sensors connected to USB ports.
    /// Accepts behaviors which define what to do during the various detection phases and when the detection process is finished (e.g. connect to a specific sensor).
    /// </summary>
    public class NithUSBportDetector
    {
        /// <summary>
        /// Pattern to search for in the incoming data to detect a NITH sensor.
        /// </summary>
        public const string NithPattern = "$Nith";

        private const int MaxTrials = 5;
        private const int TimeoutMilliseconds = 1000;
        private readonly Dictionary<string, string> _portsAndSensorNames;
        private readonly Dictionary<SerialPort, int> _portsAndTrials;
        private bool _isScanning = false;
        private List<string> _portNames;
        // Serial port, number of trials

        // Port name, sensor name
        private int _scannedPorts = 0;

        private NithPortDetectorStatus _status;

        private System.Timers.Timer _timeOutTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NithUSBportDetector"/> class.
        /// </summary>
        public NithUSBportDetector()
        {
            _portsAndTrials = new Dictionary<SerialPort, int>();
            _portNames = [];
            _portsAndSensorNames = new Dictionary<string, string>();
        }

        /// <summary>
        /// List of behaviors to execute during the detection process.
        /// </summary>
        public List<INithUSBportDetectorBehavior> Behaviors { get; set; } = [];

        /// <summary>
        /// Scans the USB ports in search of NITH sensors.
        /// </summary>
        /// <returns><c>true</c> on a succesful scan; <c>false</c> if this method is called while a scanning process is already ongoing</returns>
        public bool Scan()
        {
            // Block if already doing
            if (_isScanning) { Console.WriteLine("Error: already scanning!"); return false; }
            _isScanning = true;

            Console.WriteLine("Scanning USB ports in search of NITH sensors");

            // Notify Behaviors that scanning has started
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorStatus.Scanning, _portsAndSensorNames);
            }

            // Close all ports for safety, and clear detected ports
            CloseAllPorts();
            ClearDetectedSensors();

            // Initialize scanned ports number to zero
            _scannedPorts = 0;

            // List all port names
            _portNames = SerialPort.GetPortNames().ToList();

            // Create and open a port for each portName
            foreach (var portName in _portNames)
            {
                // Initialize the port
                SerialPort serialPort = new();
                serialPort.BaudRate = 115200;
                serialPort.Parity = Parity.Even;
                serialPort.DataBits = 8;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.PortName = portName; // assign name
                serialPort.DataReceived += SerialPort_DataReceived;

                // Add the port to the list
                var alreadyIn = false;
                foreach (var entry in _portsAndTrials)
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
                        _portsAndTrials.Add(serialPort, 0);
                    }
                    catch
                    {
                        FinishedWithError();
                    }
                }
            }

            // Small tweak just to avoid "collection modified" error
            var copy = new Dictionary<SerialPort, int>(_portsAndTrials.Count);
            foreach (var entry in _portsAndTrials)
            {
                copy.Add(entry.Key, entry.Value);
            }

            // Open all ports
            foreach (var entry in copy)
            {
                var port = entry.Key;
                try
                {
                    port.Open();
                    port.DiscardInBuffer(); // clear buffer
                }
                catch
                {
                    // if couldn't open, well, no problem. This port has been scanned, result negative
                    _scannedPorts++;
                    port.Close();
                }
            }

            // Set scan timeout timer
            if (_timeOutTimer != null) _timeOutTimer.Dispose();
            _timeOutTimer = new();
            _timeOutTimer.Interval = TimeoutMilliseconds;
            _timeOutTimer.Elapsed += Timer_Elapsed;
            _timeOutTimer.Start();

            return true; // If everything went well, return true
        }

        private void ClearDetectedSensors()
        {
            _portsAndSensorNames.Clear();
        }

        private void CloseAllPorts()
        {
            foreach (var entry in _portsAndTrials)
            {
                entry.Key.Close();
                entry.Key.Dispose();
            }
        }

        private void FinishedWithError()
        {
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorStatus.Error, _portsAndSensorNames);
            }
            _isScanning = false;
        }

        private void FinishedWithSuccess()
        {
            // Stop timeout timer
            _timeOutTimer.Stop();
            _timeOutTimer.Dispose();

            // List ports
            ListNithSensorsAndPorts();

            // Send to Behaviors
            foreach (var behavior in Behaviors)
            {
                behavior.ProcessInformation(NithPortDetectorStatus.Finished, _portsAndSensorNames);
            }
            _isScanning = false;
        }

        private string GetPortsAndSensorsList()
        {
            // Initialize an empty StringBuilder
            StringBuilder sb = new();

            // Iterate through all key-value pairs in the dictionary
            foreach (var pair in _portsAndSensorNames)
            {
                // Append the port and sensor name to the StringBuilder
                sb.Append($"{pair.Key}: {pair.Value}");

                // Append a newline character to separate each entry
                // Remove this line if you want everything on a single line
                sb.AppendLine();
            }

            // Convert the StringBuilder to a single string
            return sb.ToString().TrimEnd(); // TrimEnd to remove the last newline character
        }

        private void ListNithSensorsAndPorts()
        {
            Console.WriteLine("USB scan completed.");
            // Check if the dictionary is not null or empty
            if (_portsAndSensorNames != null && _portsAndSensorNames.Count > 0)
            {
                StringBuilder sb = new();

                // Iterate over the dictionary and append each key-value pair to the StringBuilder
                foreach (var pair in _portsAndSensorNames)
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

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Typecast
            var port = sender as SerialPort;

            // Read
            var line = port.ReadLine();

            // Increment trials
            _portsAndTrials[port]++;

            // Check...
            if (line.Contains(NithPattern))
            {
                try
                {
                    // Is it splittable?
                    var sensorName = line.Split('|')[0].Replace("$", "");

                    // SUCCESS!
                    // Add to results (only if not already in)
                    if (!_portsAndSensorNames.ContainsKey(port.PortName))
                    {
                        _portsAndSensorNames.Add(port.PortName, sensorName);
                    }

                    // Close and increment scanned ports
                    port.Close();
                    _scannedPorts++;
                }
                catch
                {
                    // Do nothing. Wait next round.
                }
            }

            // If trial numbers expired, close.
            if (_portsAndTrials[port] >= MaxTrials)
            {
                port.Close();
                _scannedPorts++;
            }

            // If process ended (scanned ports finished), it's done!
            if (_scannedPorts == _portsAndTrials.Count)
            {
                FinishedWithSuccess();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeOutTimer.Stop();
            CloseAllPorts();
            _timeOutTimer.Dispose();

            if (_isScanning)
                FinishedWithSuccess();
        }
    }
}