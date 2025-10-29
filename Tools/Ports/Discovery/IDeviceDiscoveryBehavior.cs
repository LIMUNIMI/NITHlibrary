namespace NITHlibrary.Tools.Ports.Discovery
{
    /// <summary>
    /// Interface for behaviors that respond to device discovery events.
    /// </summary>
    public interface IDeviceDiscoveryBehavior
    {
        /// <summary>
        /// Called when a NITH device is discovered on the network.
        /// </summary>
        /// <param name="device">Information about the discovered device.</param>
        void OnDeviceDiscovered(DeviceInfo device);
    }
}
