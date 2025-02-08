# NITHlibrary

__A C# Library for Interfacing NITH Sensor Peripherals and Wrappers__

<div align="center">
  <img src="NithLogo_Black_Trace.png" alt="NITH logo." width="150px"/>
</div>

NITHlibrary is the main component of the NITH framework, a framework for developing accessible software and tools dedicated to individuals with tetraplegia.

It's **Written in C# .NET Core 8** Ensuring cross-platform compatibility with Windows, Linux, and macOS.

Its **primary Purpose** is to manage communication with [NITHsensors and NITHwrappers](https://neeqstock.notion.site/NITHsensors-and-NITHwrappers-56ab43db493a423f9e8823af04fa9c46)

The **core component** of the library is the `NithModule`, which handles incoming data from sensors and parses it. `NithModule` can receive data from USB through an instance of `USBportReceiver` or from UDP through an instance of `UDPportReceiver`.

The remainder of the library contains **tools and Utilities** to facilitate the development of accessible applications using NITH sensors.

NITHlibrary proposes a functional structure for organizing your application's code, based on [NITHtemplate](https://github.com/LIMUNIMI/NITHtemplate). This structure involves defining a `Rack`, a static singleton class containing all the modules that make up the application, allowing them to be referenced from any point in the code.

Modules generally receive data in some manner. Most "modules" in the NITH framework libraries (including NITHlibrary) accept a series of "Behaviors". Behaviors are classes that implement a specific interface, defining actions to be taken with the incoming data. Each module can accept an unlimited number of behaviors and will forward all data to the various behaviors in the list. This design allows for easy and runtime modification of the application's behavior.

## How to use
NITHlibrary is built using C# within __Microsoft Visual Studio__. We recommend using the same for an optimal development experience.
In order to use NITHlibrary you can simply clone this repo, place its folder next to your project folder and add a reference to it within your project (and/or add it to your __Solution__ in __Visual Studio__).

- [ ] Precompiled DLLs will be available soon.

## Contents overview

The following is a brief explanation of the main components. See the documentation within the code for more explanations on the available tools.

### Port Receivers
These receive and redirect incoming data to the listeners.

`NithModule` instances need to be connected (i.e. added as a listener) to a port receiver in order to receive data.

`USBreceiver` is a class that manages serial communication over COM ports using the .NET SerialPort. It allows you to:

- **Connect**: Opens a serial connection to the specified COM port (e.g., COM1, COM2, etc.). If the port is already open, it closes and reopens it. It also sets up a timeout mechanism that disconnects if no data is received within a given interval.
- **Add Listeners**: Maintains a list of registered listeners (objects implementing the IPortListener interface). When data is received through the serial port, the data is forwarded to all registered listeners via their ReceivePortData method.
- **Disconnect**: Closes the serial port connection and updates the connection status accordingly.
- **Data Handling**: Uses the DataReceived event to read lines from the port and immediately transfer them to all listeners. A timeout timer is used to automatically disconnect if data is not received in time.

`UDPreceiver` is a class designed for receiving data over UDP. It operates as follows:

- **Connect**: Initializes a UdpClient on the specified port (default is 20100) and begins listening for incoming UDP packets. If the connection is successful, it maintains an open socket for receiving data.
- **Add Listeners**: Like USBreceiver, it maintains a list of listeners that implement the IPortListener interface. When a UDP packet is received, the packet (after being decoded from UTF-8) is forwarded to all registered listeners.
- **Automatic Reconnection**: After each asynchronous receive operation (or in case of an error), the UDPreceiver automatically starts another asynchronous receive call, ensuring continuous data monitoring until disconnected.
- **Disconnect and Cleanup**: Provides proper methods to disconnect and dispose of resources to release the UDP port when no longer needed.

### NithModule

At the core is the **NithModule**. This class is responsible for:
- **Data Acquisition**: Receiving raw data strings from NITH sensors.
- **Data Parsing**: Converting raw sensor input into structured `NithSensorData` objects which encapsulate details such as the sensor’s name, version, status code, and a collection of parameter values.
- **Behavior Dispatch**: Distributing parsed sensor data to two sets of behaviors:
  - **Sensor Behaviors** (defined by the `INithSensorBehavior` interface) process the sensor data.
  - **Error Behaviors** (defined by the `INithErrorBehavior` interface) handle any issues that occur during data processing, such as connection problems, protocol violations, or missing parameters.

In addition, the **NithModule** can filter incoming data using configurable lists of expected sensor names, versions, and parameters. This ensures that only valid and supported sensor data is processed; otherwise, the relevant error behavior is triggered.

### Sensor Data

Sensor data is managed through the `NithSensorData` object, which includes:
- The original raw data string.
- Metadata such as sensor name, version, and status code.
- A list of parameter values, each represented by an `INithParameterValue` structure. This structure provides not only the base and maximum values for each parameter but also a normalized value (ranging from 0 to 1) when applicable.
- Utility methods like `GetParameterValue()` and `ContainsParameter()` which assist in accessing and verifying the presence of specific data parameters.

### Sensor Behaviors

Implemented via the `INithSensorBehavior` interface, sensor behaviors define how incoming sensor data is processed. Developers can tailor these behaviors by:
- Specifying a list of expected parameters that the behavior can handle.
- Using runtime checks within the behavior’s `HandleData` method to verify the presence of these parameters.
- Extracting and processing parameter values to drive application logic.

A range of behaviors can be implemented, allowing for dynamic and flexible handling of various sensor events.

### Error Behaviors

Error behaviors, defined by the `INithErrorBehavior` interface, manage different types of errors that may occur during sensor data acquisition or parsing. Possible error types include:
- **Connection Issues**
- **Protocol Non-compliance**
- **Unexpected Sensor Names or Versions**
- **Missing Parameters**

By handling these errors through dedicated behaviors, developers can provide appropriate responses or notifications when problems occur.

### Preprocessors

Preprocessors (via the `INithPreprocessor` interface) prepare sensor data prior to its consumption by behaviors. They can modify the `NithSensorData` object by filtering or transforming its list of parameters. The order in which preprocessors are added determines the sequence of their operations, allowing for systematic data adjustments such as calibration or noise filtering.

The following Preprocessors are included in the library and serve as an example.

#### NithPreprocessor_HeadTrackerCalibrator

The `NithPreprocessor_HeadTrackerCalibrator` is designed to facilitate the calibration of head tracker data. It is specifically tailored for head trackers that detect head rotation across three axes: yaw, pitch, and roll.

- **Calibration**: This preprocessor enables users to establish a center position, allowing for the calibration of incoming data to that center through angular transformations. This ensures that the head movement data is accurate and centered.

#### NithPreprocessor_MAfilterParams

The `NithPreprocessor_MAfilterParams` implements an exponentially decaying moving average filter for filtering specified parameters. This preprocessor is useful for smoothing out noisy data.

- **Filtering**: Parameters to be filtered are defined in a list provided during the instantiation of the preprocessor. The filter applies an exponential decay to the data, reducing noise and providing a smoother signal.

#### NithPreprocessor_WebcamWrapper

The `NithPreprocessor_WebcamWrapper` is specifically designed for use with the `NITHfacecamWrapper`. It performs several functions related to eye and mouth aperture detection.

- **Calibration**: Calibrates eye and mouth apertures, taking into account the user's maximum and minimum aperture levels. This ensures that the aperture values are normalized to a range from 0 to 1.

- **Boolean Extraction**: Extracts boolean values for eye and mouth apertures, indicating whether the aperture exceeds a specified threshold. This is useful for detecting open/closed states.

### Additional Tools

The library also includes various utility tools organized in distinct namespaces:
- **Filters**: Provide functions for data smoothing (e.g., moving average filters).
- **Mappers**: Assist in transforming numerical data, such as remapping a sensor’s output range.
- **Other Utilities**: These may include helpers for tasks like angle conversions or calculating velocity from sensor readings.

Overall, NITHlibrary abstracts sensor communication and data processing with a modular, behavior-driven architecture. This design not only promotes flexibility and ease of integration but also enables developers to rapidly prototype and extend accessible applications for users with specialized needs.
  