using NITHlibrary.Nith.Wrappers;
using NITHlibrary.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NITHlibrary.Tools.Ports.Discovery
{
    /// <summary>
    /// Service that listens for NITH device discovery broadcasts on port 20500.
    /// Responds to devices with the receiver's IP and expected port, and notifies behaviors.
    /// </summary>
    public class NithDeviceDiscoveryService : IDisposable
    {
        /// <summary>
        /// Port used for device discovery communication.
        /// </summary>
        public const int DiscoveryPort = 20500;

        private UdpClient _listener;
        private bool _isRunning = false;
        private readonly object _devicesLock = new();
        private readonly Dictionary<string, DeviceInfo> _discoveredDevices = new();

        /// <summary>
        /// Gets the list of behaviors to execute when a device is discovered.
        /// </summary>
        public List<IDeviceDiscoveryBehavior> Behaviors { get; } = new();

        /// <summary>
        /// Gets a read-only collection of all discovered devices.
        /// Key is DeviceType-DeviceIP for uniqueness.
        /// </summary>
        public IReadOnlyDictionary<string, DeviceInfo> DiscoveredDevices
        {
            get
            {
                lock (_devicesLock)
                {
                    return new Dictionary<string, DeviceInfo>(_discoveredDevices);
                }
            }
        }

        /// <summary>
        /// Starts the discovery service listening on port 20500.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                Console.WriteLine("Discovery service already running.");
                return;
            }

            try
            {
                _listener = new UdpClient(DiscoveryPort);
                _isRunning = true;
                Console.WriteLine($"Device Discovery Service started on port {DiscoveryPort}");

                // Begin listening asynchronously
                _listener.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                _isRunning = false;
            }
        }

        /// <summary>
        /// Stops the discovery service.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _listener?.Close();
            Console.WriteLine("Device Discovery Service stopped.");
        }

        /// <summary>
        /// Adds a behavior to be notified when devices are discovered.
        /// </summary>
        public void AddBehavior(IDeviceDiscoveryBehavior behavior)
        {
            if (!Behaviors.Contains(behavior))
            {
                Behaviors.Add(behavior);
            }
        }

        /// <summary>
        /// Removes a behavior from the notification list.
        /// </summary>
        public void RemoveBehavior(IDeviceDiscoveryBehavior behavior)
        {
            Behaviors.Remove(behavior);
        }

        /// <summary>
        /// Disposes the discovery service and releases resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _listener?.Dispose();
            Behaviors.Clear();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Callback for receiving UDP messages.
        /// </summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            if (!_isRunning)
                return;

            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, DiscoveryPort);
                byte[] receivedBytes = _listener.EndReceive(result, ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(receivedBytes);

                // Process the discovery message
                ProcessDiscoveryMessage(message, remoteEndPoint.Address.ToString());

                // Continue listening
                if (_isRunning)
                {
                    _listener.BeginReceive(ReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);

                // Try to continue listening
                if (_isRunning && _listener != null)
                {
                    try
                    {
                        _listener.BeginReceive(ReceiveCallback, null);
                    }
                    catch
                    {
                        // If we can't continue, stop the service
                        _isRunning = false;
                    }
                }
            }
        }

        /// <summary>
        /// Processes a device discovery message.
        /// Expected format: "devicename-version|device_ip=ip&device_port=port"
        /// </summary>
        private void ProcessDiscoveryMessage(string message, string senderIp)
        {
            try
            {
                // Split by '|' to separate header from parameters
                var parts = message.Split('|');
                if (parts.Length != 2)
                {
                    Console.WriteLine($"Invalid discovery message format: {message}");
                    return;
                }

                // Parse header: "devicename-version"
                var header = parts[0];
                var headerParts = header.Split('-');
                if (headerParts.Length != 2)
                {
                    Console.WriteLine($"Invalid device header format: {header}");
                    return;
                }

                string deviceType = headerParts[0];
                string version = headerParts[1];

                // Parse parameters: "device_ip=ip&device_port=port"
                var parameters = ParseParameters(parts[1]);
                if (!parameters.ContainsKey("device_ip") || !parameters.ContainsKey("device_port"))
                {
                    Console.WriteLine($"Missing required parameters in discovery message: {message}");
                    return;
                }

                string deviceIp = parameters["device_ip"];
                if (!int.TryParse(parameters["device_port"], out int devicePort))
                {
                    Console.WriteLine($"Invalid device_port value: {parameters["device_port"]}");
                    return;
                }

                // Look up the expected receiver port for this device type
                int expectedReceiverPort = GetExpectedReceiverPort(deviceType);
                if (expectedReceiverPort == 0)
                {
                    Console.WriteLine($"Unknown device type: {deviceType}");
                    return;
                }

                // Create device info
                var deviceInfo = new DeviceInfo
                {
                    DeviceType = deviceType,
                    Version = version,
                    DeviceIP = deviceIp,
                    DevicePort = devicePort,
                    ExpectedReceiverPort = expectedReceiverPort,
                    DiscoveryTime = DateTime.Now
                };

                // Store or update the device
                string deviceKey = $"{deviceType}-{deviceIp}";
                lock (_devicesLock)
                {
                    _discoveredDevices[deviceKey] = deviceInfo;
                }

                Console.WriteLine($"Device discovered: {deviceInfo}");

                // Send response back to the device
                SendDiscoveryResponse(deviceIp, expectedReceiverPort);

                // Notify behaviors
                NotifyBehaviors(deviceInfo);
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                Console.WriteLine($"Error processing discovery message: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses parameters in the format "key1=value1&key2=value2".
        /// </summary>
        private Dictionary<string, string> ParseParameters(string parametersString)
        {
            var result = new Dictionary<string, string>();
            var pairs = parametersString.Split('&');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    result[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the expected receiver port for a given device type by looking up the enum.
        /// </summary>
        private int GetExpectedReceiverPort(string deviceType)
        {
            try
            {
                // Try to parse the device type as an enum value
                if (Enum.TryParse<NithWrappersReceiverPorts>(deviceType, out var port))
                {
                    return (int)port;
                }
            }
            catch
            {
                // Enum parsing failed
            }

            return 0; // Unknown device type
        }

        /// <summary>
        /// Sends a response to the discovered device.
        /// Format: "NITHreceiver|receiver_ip=ip&expected_port=port"
        /// </summary>
        private void SendDiscoveryResponse(string deviceIp, int expectedPort)
        {
            try
            {
                // Get the local IP address on the same subnet as the device
                string localIp = GetLocalIpAddressForSubnet(deviceIp);

                // Build response message
                string responseMessage = $"NITHreceiver|receiver_ip={localIp}&expected_port={expectedPort}";

                // Send UDP message to the device on port 20500
                // Important: Bind to specific local address to ensure response routes correctly
                using (var sender = new UdpClient(new IPEndPoint(IPAddress.Parse(localIp), 0)))
                {
                    var endPoint = new IPEndPoint(IPAddress.Parse(deviceIp), DiscoveryPort);
                    byte[] data = Encoding.UTF8.GetBytes(responseMessage);
                    sender.Send(data, data.Length, endPoint);
                }

                Console.WriteLine($"Sent discovery response to {deviceIp}: {responseMessage}");
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
                Console.WriteLine($"Error sending discovery response: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the local IP address that is on the same subnet as the target IP.
        /// </summary>
        private string GetLocalIpAddressForSubnet(string targetIp)
        {
            try
            {
                var targetAddress = IPAddress.Parse(targetIp);
                var hostName = Dns.GetHostName();
                var hostAddresses = Dns.GetHostAddresses(hostName);

                // Find an IPv4 address on the same subnet
                foreach (var address in hostAddresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Check if on same /24 subnet (simple heuristic)
                        var localBytes = address.GetAddressBytes();
                        var targetBytes = targetAddress.GetAddressBytes();

                        if (localBytes[0] == targetBytes[0] &&
                            localBytes[1] == targetBytes[1] &&
                            localBytes[2] == targetBytes[2])
                        {
                            return address.ToString();
                        }
                    }
                }

                // If no match on same subnet, return first available IPv4 address
                var firstIpv4 = hostAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                if (firstIpv4 != null)
                {
                    return firstIpv4.ToString();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Log(ex);
            }

            // Fallback to localhost
            return "127.0.0.1";
        }

        /// <summary>
        /// Notifies all registered behaviors about the discovered device.
        /// </summary>
        private void NotifyBehaviors(DeviceInfo deviceInfo)
        {
            foreach (var behavior in Behaviors)
            {
                try
                {
                    behavior.OnDeviceDiscovered(deviceInfo);
                }
                catch (Exception ex)
                {
                    LoggingService.Log(ex);
                    Console.WriteLine($"Error in discovery behavior: {ex.Message}");
                }
            }
        }
    }
}
