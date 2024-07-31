using System.Net;
using System.Net.Sockets;

namespace NITHlibrary.Tools.Ports
{
    /// <summary>
    /// Provides functionality to send messages via UDP.
    /// </summary>
    public class UDPsender : IDisposable
    {
        /// <summary>
        /// Gets or sets the port number to send UDP messages to.
        /// </summary>
        public int Port { get; set; }
        private UdpClient _client;
        private readonly IPEndPoint _endPoint;

        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPsender"/> class with the specified port.
        /// </summary>
        /// <param name="port">The port number to send UDP messages to.</param>
        public UDPsender(int port)
        {
            Port = port;
            _client = new();
            _endPoint = new(IPAddress.Broadcast, Port);
        }

        /// <summary>
        /// Sends a message to the specified UDP port.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ObjectDisposedException">Thrown when attempting to use a disposed object.</exception>
        public void SendMessage(string message)
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException("UDPsenderModule");
            }

            var bytes = System.Text.Encoding.ASCII.GetBytes(message);
            _client.Send(bytes, bytes.Length, _endPoint);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UDPsender"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _client.Close();
                    _client = null;
                }
            }
            _disposedValue = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="UDPsender"/> class.
        /// Should be disposed to ensure the UDP port is released.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}