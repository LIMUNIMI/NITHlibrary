using NITHlibrary.Tools.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NITHlibrary.Tools.Ports
{
    /// <summary>
    /// Class for receiving UDP packets.
    /// Implements <see cref="IDisposable"/> for proper cleanup of resources.
    /// Accepts a list of listeners to notify when data is received.
    /// </summary>
    public class UDPreceiver : IDisposable
    {
        /// <summary>
        /// Default UDP port used by the receiver.
        /// </summary>
        public const int DefaultNithUdpPort = 20100;

        private UdpClient _client;
        private int _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPreceiver"/> class with the specified port.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public UDPreceiver(int port = DefaultNithUdpPort)
        {
            this._port = port;
        }

        /// <summary>
        /// Gets a value indicating whether the receiver is connected.
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Gets or sets the list of listeners to receive port data notifications.
        /// </summary>
        public List<IPortListener> Listeners { get; set; } = [];

        /// <summary>
        /// Gets or sets the port number for the receiver.
        /// Automatically adjusts if the value is less than 1.
        /// </summary>
        public int Port
        {
            get => _port;
            set
            {
                if (value < 1) _port = 1;
                else _port = value;
            }
        }

        /// <summary>
        /// Connects the UDP receiver to the specified port.
        /// </summary>
        public void Connect()
        {
            IsConnected = InitializeUdp();
        }

        /// <summary>
        /// Disconnects the UDP receiver.
        /// </summary>
        public void Disconnect()
        {
            _client?.Close();
            IsConnected = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Should be disposed to ensure UDP port is released.
        /// </summary>
        public void Dispose()
        {
            _client?.Close();
            _client?.Dispose();
            GC.SuppressFinalize(this); // Added this line to address CA1816
        }

        /// <summary>
        /// Initializes the UDP client and begins listening for incoming data.
        /// </summary>
        /// <returns><c>true</c> if the initialization succeeded, otherwise <c>false</c>.</returns>
        private bool InitializeUdp()
        {
            _client?.Close();
            _client?.Dispose();

            // Client uses as receive udp client
            _client = new(Port);

            try
            {
                _client.BeginReceive(new(Receive), null);
                Console.WriteLine("UDP port connected!");
                return true;
            }
            catch (Exception e)
            {
                LoggingService.Log(e);
                return false;
            }
        }

        /// <summary>
        /// Notifies all listeners with the received data.
        /// </summary>
        /// <param name="line">The data received from the UDP port.</param>
        private void NotifyListeners(string line)
        {
            foreach (var listener in Listeners)
            {
                listener.ReceivePortData(line);
            }
        }

        /// <summary>
        /// Callback method invoked when data is received on the UDP port.
        /// </summary>
        /// <param name="res">The result of the asynchronous receive operation.</param>
        private void Receive(IAsyncResult res)
        {
            if (!IsConnected)
                return;

            try
            {
                IPEndPoint remoteIpEndPoint = new(IPAddress.Any, Port);
                var received = _client.EndReceive(res, ref remoteIpEndPoint);
                NotifyListeners(Encoding.UTF8.GetString(received));
            }
            catch (Exception e)
            {
                LoggingService.Log(e);
                // Decide if you want to stop on certain types of exceptions
            }
            finally
            {
                // Solution to reconnect automatically on error
                if (_client != null && IsConnected)
                {
                    _client.BeginReceive(new(Receive), null);
                }
            }
        }
    }
}