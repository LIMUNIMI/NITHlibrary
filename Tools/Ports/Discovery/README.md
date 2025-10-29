# NITH Device Discovery Service

## Overview

The `NithDeviceDiscoveryService` enables automatic discovery and configuration of NITH devices (like NITHphoneWrapper) on the local network.

## How It Works

1. **Device Broadcasts Discovery**: A NITH device (e.g., phone app) sends a UDP broadcast message on port **20500**:
   ```
   NITHphoneWrapper-1.0|device_ip=192.168.1.100&device_port=21103
   ```

2. **Service Receives and Processes**: The `NithDeviceDiscoveryService` listening on port 20500:
   - Parses the device type, version, IP, and port
   - Looks up the expected receiver port from `NithWrappersReceiverPorts` enum
   - Stores the device info in a registry

3. **Service Responds**: Sends a response back to the device on port 20500:
   ```
   NITHreceiver|receiver_ip=192.168.1.50&expected_port=20103
   ```

4. **Behaviors Are Notified**: All registered behaviors receive the device info for custom handling

## Message Formats

### Device Discovery Message
```
devicename-version|device_ip=ip&device_port=port
```
Example:
```
NITHphoneWrapper-1.0|device_ip=192.168.1.100&device_port=21103
```

### Receiver Response Message
```
NITHreceiver|receiver_ip=ip&expected_port=port
```
Example:
```
NITHreceiver|receiver_ip=192.168.1.50&expected_port=20103
```

## Usage Example

### Basic Setup

```csharp
using NITHlibrary.Tools.Ports.Discovery;

// Create and start the discovery service
var discoveryService = new NithDeviceDiscoveryService();

// Add custom behaviors
discoveryService.AddBehavior(new UpdateHeadBowerPhoneSettings());

// Start listening
discoveryService.Start();

// Later, when done...
discoveryService.Stop();
discoveryService.Dispose();
```

### Creating a Custom Behavior

```csharp
public class UpdateHeadBowerPhoneSettings : IDeviceDiscoveryBehavior
{
    public void OnDeviceDiscovered(DeviceInfo device)
    {
        if (device.DeviceType == "NITHphoneWrapper")
        {
            // Update your UDPsender with the device IP and port
            Rack.UDPsenderPhone.SetIpAddress(device.DeviceIP);
            Rack.UDPsenderPhone.Port = device.DevicePort;
            
            // Update UI settings (if needed)
            // MainWindow.txtPhoneIP.Text = device.DeviceIP;
            // MainWindow.txtPhonePort.Text = device.DevicePort.ToString();
            
            Console.WriteLine($"Phone auto-configured: {device.DeviceIP}:{device.DevicePort}");
        }
    }
}
```

## Architecture

### Classes

- **`NithDeviceDiscoveryService`**: Main service class that listens on port 20500
- **`DeviceInfo`**: Data class containing discovered device information
- **`IDeviceDiscoveryBehavior`**: Interface for custom behaviors

### Key Features

- ? UDP-based discovery on port 20500
- ? Automatic port lookup from `NithWrappersReceiverPorts` enum
- ? Device registry with updates when same device re-broadcasts
- ? Behavior pattern for extensibility
- ? Subnet-aware IP detection (responds with local IP on same subnet)
- ? Thread-safe device registry

## Port Mapping

The service automatically maps device types to receiver ports using the `NithWrappersReceiverPorts` enum:

```csharp
public enum NithWrappersReceiverPorts
{
    NITHwebcamWrapper = 20100,
    NITHbeamEyeTrackerWrapper = 20101,
    NITHphoneWrapper = 20103,
}
```

When a device like "NITHphoneWrapper" is discovered, the service responds with `expected_port=20103`.

## Network Details

- **Discovery Port**: 20500 (both send and receive)
- **Protocol**: UDP
- **Subnet Detection**: Service responds with local IP on same /24 subnet as device
- **Device Updates**: If same device broadcasts again, registry is updated (no duplicate handling)

## Integration Example for HeadBower

```csharp
// In DefaultSetup.cs or similar

// Create discovery service (typically in Rack or as singleton)
var phoneDiscovery = new NithDeviceDiscoveryService();

// Create behavior to update phone connection when discovered
phoneDiscovery.AddBehavior(new PhoneAutoConfigBehavior 
{
    UdpSender = Rack.UDPsenderPhone,
    SettingsWindow = Rack.InstrumentWindow
});

// Start the service
phoneDiscovery.Start();

// Add to disposables
disposables.Add(phoneDiscovery);
```

## Notes

- Simple design: No heartbeat, disconnection handling, or multi-device support (for now)
- Devices can press the discovery button multiple times to update their info
- All discovered devices are stored in `DiscoveredDevices` dictionary
- Behaviors are invoked on network thread - handle UI updates appropriately (use Dispatcher if needed)
