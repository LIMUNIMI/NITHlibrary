using System.IO.Ports;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NITHlibrary.Tools.Ports
{
    public class USBreceiver : IDisposable
    {
        public const string PortPrefix = "COM";
        private readonly SerialPort serialPort;
        private int port = 1;
        private Timer timeoutTimer;

        public USBreceiver(int baudRate = 115200, int disconnectTimeout = 1500)
        {
            timeoutTimer = new Timer();
            DisconnectTimeout = disconnectTimeout;
            timeoutTimer.Elapsed += TimeoutTimer_Elapsed;

            serialPort = new SerialPort();
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public int DisconnectTimeout
        {
            get
            { return (int)timeoutTimer.Interval; }
            set
            { timeoutTimer.Interval = value; }
        }

        public bool IsConnected { get; private set; } = false;

        public List<IPortListener> Listeners { get; set; } = new List<IPortListener>();

        public int Port
        {
            get { return port; }
            set
            {
                if (value < 1) port = 1;
                else port = value;
            }
        }

        public bool ReadError { get; protected set; } = false;

        public bool Connect(int port)
        {
            Port = port;
            return Connect();
        }

        public bool Connect()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }

            serialPort.PortName = PortPrefix + Port.ToString();

            try
            {
                serialPort.Open();
            }
            catch
            {
                IsConnected = false;
                return false;
            }

            IsConnected = true;
            return true;
        }

        public bool Disconnect()
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
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

        public void Dispose()
        {
            serialPort.Dispose();
        }

        public void Write(string str)
        {
            if (IsConnected)
                serialPort.Write(str);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            timeoutTimer.Stop();

            try
            {
                TransferDataToListeners(serialPort.ReadLine());
            }
            catch
            {
                // If data is not arriving...
            }

            timeoutTimer.Start();
        }

        private void TimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutTimer.Stop();
            Disconnect();
            Console.WriteLine("USB port timeout: disconnected.");
        }

        private void TransferDataToListeners(string line)
        {
            foreach (IPortListener listener in Listeners)
            {
                listener.ReceivePortData(line);
            }
        }
    }
}