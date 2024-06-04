using System.Net;
using System.Net.Sockets;

namespace NITHlibrary.Tools.Ports
{
    public class UDPsender : IDisposable
    {
        public int Port { get; set; }
        private UdpClient client;
        private IPEndPoint endPoint;

        private bool disposedValue = false; // To detect redundant calls

        public UDPsender(int port)
        {
            Port = port;
            client = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Broadcast, Port);
        }

        public void SendMessage(string message)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("UDPsenderModule");
            }

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);
            client.Send(bytes, bytes.Length, endPoint);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Close();
                    client = null;
                }
            }
            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}