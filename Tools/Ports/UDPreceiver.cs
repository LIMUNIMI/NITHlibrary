using NITHlibrary.Tools.Logging;
using System.Diagnostics;
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
        private long _lastSampleTicks = 0;
        private long _minTicksBetweenSamples = 0;
        private int _droppedSamplesCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPreceiver"/> class with the specified port.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public UDPreceiver(int port = DefaultNithUdpPort)
        {
            this._port = port;
            MaxSamplesPerSecond = 100; // Default rate limit
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
        /// Gets or sets the maximum number of samples per second to process.
        /// Set to 0 for unlimited (no rate limiting).
        /// Default is 100 samples/second.
        /// </summary>
        public int MaxSamplesPerSecond
        {
            get => _minTicksBetweenSamples == 0 ? 0 : (int)(Stopwatch.Frequency / _minTicksBetweenSamples);
            set
            {
                if (value <= 0)
                {
                    _minTicksBetweenSamples = 0; // Unlimited
                }
                else
                {
                    _minTicksBetweenSamples = Stopwatch.Frequency / value;
                }
            }
        }

        /// <summary>
        /// Gets the total number of samples dropped due to rate limiting since connection.
        /// </summary>
        public int DroppedSamplesCount => _droppedSamplesCount;

        /// <summary>
        /// Resets the dropped samples counter.
        /// </summary>
        public void ResetDroppedSamplesCount()
        {
            _droppedSamplesCount = 0;
        }

        /// <summary>
        /// Connects the UDP receiver to the specified port.
        /// </summary>
        public void Connect()
        {
            IsConnected = InitializeUdp();
            _lastSampleTicks = 0;
            _droppedSamplesCount = 0;
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
            GC.SuppressFinalize(this);
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
            // Rate limiting: check if enough time has passed since last sample
            if (_minTicksBetweenSamples > 0)
            {
                long currentTicks = Stopwatch.GetTimestamp();
                long ticksSinceLastSample = currentTicks - _lastSampleTicks;

                if (_lastSampleTicks != 0 && ticksSinceLastSample < _minTicksBetweenSamples)
                {
                    // Drop this sample - too fast
                    _droppedSamplesCount++;
                    return;
                }

                _lastSampleTicks = currentTicks;
            }

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