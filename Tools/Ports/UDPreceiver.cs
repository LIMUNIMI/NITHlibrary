using System.Net;
using System.Net.Sockets;
using System.Text;
using NITHlibrary.Tools.Logging;

namespace NITHlibrary.Tools.Ports
{
    public class UDPreceiver : IDisposable
    {
        public const int DEFAULT_NITH_UDP_PORT = 20100;
        private UdpClient Client;
        private int port;

        public UDPreceiver(int port = DEFAULT_NITH_UDP_PORT)
        {
            this.port = port;
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

        public void Connect()
        {
            IsConnected = InitializeUdp();
        }

        public void Disconnect()
        {
            Client?.Close();
            IsConnected = false;
        }

        public void Dispose()
        {
            Client?.Close();
            Client?.Dispose();
        }

        private bool InitializeUdp()
        {
            Client?.Close();
            Client?.Dispose();

            // Client uses as receive udp client
            Client = new UdpClient(Port);

            try
            {
                Client.BeginReceive(new AsyncCallback(Receive), null);
                Console.WriteLine("UDP port connected!");
                return true;
            }
            catch (Exception e)
            {
                LoggingService.Log(e);
                return false;
            }
        }

        // CallBack
        private void Receive(IAsyncResult res)
        {
            if (!IsConnected)
                return;

            try
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);
                byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);
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
                if (Client != null && IsConnected)
                {
                    Client.BeginReceive(new AsyncCallback(Receive), null);
                }
            }
        }

        private void NotifyListeners(string line)
        {
            foreach (IPortListener listener in Listeners)
            {
                listener.ReceivePortData(line);
            }
        }
    }
}