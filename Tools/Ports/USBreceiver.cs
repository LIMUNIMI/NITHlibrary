using System.IO.Ports;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NITHlibrary.Tools.Ports
{
    /// <summary>
    /// This class represents a USB receiver that handles serial communication over a specified port.
    /// It includes mechanisms for connecting, disconnecting, reading data, and handling timeouts.
    /// </summary>
    public class USBreceiver : IDisposable
    {
        /// <summary>
        /// The prefix used for port names.
        /// </summary>
        public const string PortPrefix = "COM";

        private readonly SerialPort _serialPort;
        private int _port = 1;
        private readonly Timer _timeoutTimer;

        /// <summary>
        /// Initializes an instance of the <see cref="USBreceiver"/> class with specified baudRate and disconnectTimeout.
        /// </summary>
        /// <param name="baudRate">The baud rate for serial communication. Default is 115200.</param>
        /// <param name="disconnectTimeout">The timeout duration in milliseconds before disconnecting if no data is received. Default is 1500ms.</param>
        public USBreceiver(int baudRate = 115200, int disconnectTimeout = 1500)
        {
            _timeoutTimer = new();
            DisconnectTimeout = disconnectTimeout;
            _timeoutTimer.Elapsed += TimeoutTimer_Elapsed;

            _serialPort = new();
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived += new(DataReceivedHandler);
        }

        /// <summary>
        /// Gets or sets the disconnect timeout in milliseconds.
        /// </summary>
        public int DisconnectTimeout
        {
            get => (int)_timeoutTimer.Interval;
            set => _timeoutTimer.Interval = value;
        }

        /// <summary>
        /// Gets a value indicating whether the receiver is connected.
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Gets or sets the list of receiver listeners.
        /// </summary>
        public List<IPortListener> Listeners { get; set; } = [];

        /// <summary>
        /// Gets or sets the port number. Ensures port number is at least 1.
        /// </summary>
        public int Port
        {
            get => _port;
            set { _port = value < 1 ? 1 : value; }
        }

        /// <summary>
        /// Gets a value indicating whether a read error has occurred.
        /// </summary>
        public bool ReadError { get; protected set; } = false;

        /// <summary>
        /// Connects to the specified port.
        /// </summary>
        /// <param name="port">The port number to connect to.</param>
        /// <returns>True if connected successfully, otherwise false.</returns>
        public bool Connect(int port)
        {
            Port = port;
            return Connect();
        }

        /// <summary>
        /// Connects to the port specified by the Port property.
        /// </summary>
        /// <returns>True if connected successfully, otherwise false.</returns>
        public bool Connect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _serialPort.PortName = PortPrefix + Port.ToString();

            try
            {
                _serialPort.Open();
            }
            catch
            {
                IsConnected = false;
                return false;
            }

            IsConnected = true;
            return true;
        }

        /// <summary>
        /// Disconnects from the current port.
        /// </summary>
        /// <returns>True if disconnected successfully, otherwise false.</returns>
        public bool Disconnect()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    IsConnected = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="USBreceiver"/>.
        /// </summary>
        public void Dispose()
        {
            _serialPort.Dispose();
        }

        /// <summary>
        /// Writes data to the connected port.
        /// </summary>
        /// <param name="str">The string data to be written.</param>
        public void Write(string str)
        {
            if (IsConnected)
                _serialPort.Write(str);
        }

        /// <summary>
        /// Handles the DataReceived event and transfers the received data to the registered listeners.
        /// </summary>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            _timeoutTimer.Stop();

            try
            {
                TransferDataToListeners(_serialPort.ReadLine());
            }
            catch
            {
                // If data is not arriving safely, handle the exception.
            }

            _timeoutTimer.Start();
        }

        /// <summary>
        /// Handles the Elapsed event of the timeout timer to disconnect on timeout.
        /// </summary>
        private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeoutTimer.Stop();
            Disconnect();
            Console.WriteLine("USB port timeout: disconnected.");
        }

        /// <summary>
        /// Transfers data to all registered listeners.
        /// </summary>
        /// <param name="line">The line of data received from the serial port.</param>
        private void TransferDataToListeners(string line)
        {
            foreach (var listener in Listeners)
            {
                listener.ReceivePortData(line);
            }
        }
    }
}