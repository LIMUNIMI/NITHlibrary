using NITHlibrary.Tools.Ports.Discovery;
using System;

namespace NITHlibrary.Tools.Ports.Discovery.Examples
{
    /// <summary>
    /// Example behavior that demonstrates how to respond to device discovery events.
    /// This example simply logs the discovered device information to the console.
    /// </summary>
    public class ExampleDeviceDiscoveryBehavior : IDeviceDiscoveryBehavior
    {
        /// <summary>
        /// Called when a NITH device is discovered on the network.
        /// </summary>
        /// <param name="device">Information about the discovered device.</param>
        public void OnDeviceDiscovered(DeviceInfo device)
        {
            Console.WriteLine($"[Discovery] Device found: {device}");
            
            // Example: You could update UI elements, save to settings, etc.
            // For HeadBower phone discovery, you would:
            // 1. Update the IP address in settings
            // 2. Update the UDPsender target
            // 3. Update any UI input boxes showing the IP
        }
    }
}
