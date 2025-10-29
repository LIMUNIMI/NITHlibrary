using System;

namespace NITHlibrary.Tools.Ports.Discovery
{
    /// <summary>
    /// Represents information about a discovered NITH device.
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the device type (e.g., "NITHphoneWrapper").
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the device version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        public string DeviceIP { get; set; }

        /// <summary>
        /// Gets or sets the port the device is listening on.
        /// </summary>
        public int DevicePort { get; set; }

        /// <summary>
        /// Gets or sets the expected receiver port for this device type.
        /// This is the port where the receiver expects to receive data from this device.
        /// </summary>
        public int ExpectedReceiverPort { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this device was discovered.
        /// </summary>
        public DateTime DiscoveryTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInfo"/> class.
        /// </summary>
        public DeviceInfo()
        {
            DiscoveryTime = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the device information.
        /// </summary>
        public override string ToString()
        {
            return $"{DeviceType}-{Version} at {DeviceIP}:{DevicePort} (expects receiver port {ExpectedReceiverPort})";
        }
    }
}
