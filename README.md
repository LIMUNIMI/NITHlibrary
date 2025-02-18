# NITHlibrary

_A C# Library for Interfacing NITH Sensor Peripherals and Wrappers_

<div align="center">
  <img src="NithLogo_Black_Trace.png" alt="NITH logo." width="150px"/>
</div>
<br/>

NITHlibrary is the main component of NITH, a framework for developing accessible software and tools dedicated to individuals with tetraplegia.

It's **Written in C# .NET Core 8** Ensuring cross-platform compatibility with Windows, Linux, and macOS.

Its **primary Purpose** is to manage communication with [NITHsensors and NITHwrappers](https://neeqstock.notion.site/NITHsensors-and-NITHwrappers-56ab43db493a423f9e8823af04fa9c46)

The **core component** of the library is the `NithModule`, which handles incoming data from sensors and parses it. `NithModule` can receive data from USB through an instance of `USBportReceiver` or from UDP through an instance of `UDPportReceiver`.

The remainder of the library contains **tools and Utilities** to facilitate the development of accessible applications using NITH sensors.

NITHlibrary proposes a functional structure for organizing your application's code, based on [NITHtemplate](https://github.com/LIMUNIMI/NITHtemplate). This structure involves defining a `Rack`, a static singleton class containing all the modules that make up the application, allowing them to be referenced from any point in the code.

Modules generally receive data in some manner. Most "modules" in the NITH framework libraries (including NITHlibrary) accept a series of "Behaviors". Behaviors are classes that implement a specific interface, defining actions to be taken with the incoming data. Each module can accept an unlimited number of behaviors and will forward all data to the various behaviors in the list. This design allows for easy and runtime modification of the application's behavior.

## How to use it
NITHlibrary is built using C# within __Microsoft Visual Studio__. We recommend using the same for an optimal development experience.
In order to use NITHlibrary you can simply clone this repo, place its folder next to your project folder and add a reference to it within your project (and/or add it to your __Solution__ in __Visual Studio__).

- [ ] Precompiled DLLs will be available soon.

## Contents Overview

The following is a brief explanation of the main components. See the documentation within the code for more explanations on the available tools.

### NithModule

The `NithModule` class is the main component for managing data from NITH sensors and wrappers within the NITHlibrary.
Key features of the `NithModule` include:

- **Data Reception**: It reads raw data strings from NITH sensors and processes them into structured `NithSensorData` objects.
- **Parameter Filtering**: It supports filtering incoming data based on expected sensor names, versions, and parameters, ensuring that only relevant data triggers behaviors.
- **Preprocessing**: The module can utilize a list of preprocessors (`Preprocessors`) to transform data before it is processed further.
- **Sensor Behaviors Invocation**: Upon receiving new sensor data, it calls the appropriate behaviors (`SensorBehaviors`) to handle the data, allowing for extensible application logic.
- **Error Behaviors Invocation**: The class maintains a list of error behaviors (`ErrorBehaviors`) that are invoked when errors occur during data processing.

It implements the `IDisposable` interface to allow for efficient resource release upon program termination (must be called!).
A `NithModule` can receive data from multiple sources. For this reason, it implements the `IPortListener` interface, which allow it to be added as a listener to various types of port listeners and data receivers.

The library allows to define multiple NithModules, to manage multiple sensors at the same time.

### Ports
The `Ports` namespace includes modules designed to listen for data from USB and UDP ports. Those can send data to other components (most importantly to one - or more than one - `NITHModule`).

- **UDP Reception**: The UDP receiver is responsible for listening for incoming UDP packets on a specified port. It uses asynchronous programming to handle data reception efficiently, allowing for real-time data processing. When data is received, the UDP receiver notifies registered listeners by passing the received data to them, ensuring that the information can be processed or acted upon as needed. This implementation supports broadcast messaging, making it suitable for scenarios where multiple receivers may need to receive the same data.

- **USB Reception**: The USB receiver handles serial communication over a specified USB port. It manages the connection lifecycle, including connecting, disconnecting, and reading data from the serial port. The USB receiver also incorporates a timeout mechanism to disconnect if no data is received within a specified duration. Upon receiving data, it transfers the information to registered listeners for further processing. This allows for seamless integration of USB-based devices into the application, enabling data exchange and control.

They both implement the `IDisposable` interface to allow the implementation of resource release mechanisms after program termination.

### Preprocessors
The `Preprocessors` package provides interfaces and implementations for modifying incoming sensor data before it is processed by behaviors. The core interface, `INithPreprocessor`, defines a method to transform `NithSensorData`, allowing for various preprocessing operations, user defined. The package includes two examples: 
- `NithPreprocessor_HeadTrackerCalibrator`, which calibrates head tracker data by adjusting incoming values to a defined center position
- `NithPreprocessor_MAfilterParams`, which applies a moving average exponential decaying filter to specified parameters. 

They can then be added to the specific list in `NithModule`. The order in which preprocessors are added to the `NithModule` determines the sequence of their operations.

### Internals

The `Internals` package of the NITHlibrary contains essential interfaces, enums, and data structures that facilitate the operation of the NITH framework.

#### Behavior Interfaces

The package defines two primary interfaces for behavior handling.

**INithSensorBehavior**: This interface defines a template for a behavior that reacts to incoming sensor data (i.e. `NithSensorData`). Classes implementing this interface must provide an implementation for the `HandleData` method.

Developers can tailor these behaviors by:
- Specifying a list of expected parameters that the behavior can handle.
- Using runtime checks within the behavior’s `HandleData` method to verify the presence of these parameters.
- Extracting and processing parameter values to drive application logic.

**INithErrorBehavior**: This interface specifies how to manage errors within the system. Implementing classes must define the `HandleError` method, which takes an `NithErrors` enum value and returns a boolean indicating whether the error was handled.

Possible error types include:
- Connection Issues
- Protocol Non-compliance
- Unexpected Sensor Names or Versions
- Missing Parameters

Behaviors implementing these classes can then be added to the lists of behaviors contained in a `NithModule`.

#### Enums
Several enums are defined to represent various states and parameters:

- **NithErrors**: This enum enumerates possible errors that can occur within the `NithModule`, including connection issues, compliance errors, and parameter-related errors.

- **NithParameters**: This enum lists the parameters that can be output by a NITH sensor, covering aspects related to the eyes, mouth, head, and system calibration. Each entry represents a specific parameter that can be monitored.

- **NithStatusCodes**: This enum represents various status codes received from a NITH sensor, indicating operational status, errors, or calibration needs.

#### Data Types
- **NithSensorData**:  This class encapsulates data received from NITH sensors. This includes:
  - The original raw data string.
  - Metadata such as sensor name, version, and status code.
  - A list of parameter values, each represented by a `NithParameterValue` structure. This structure provides not only the base and maximum values for each parameter but also a normalized value (ranging from 0 to 1) when applicable.
  - Utility methods like `ContainsParameter()` which assist in accessing and verifying the presence of specific data parameters.
  - Lastly, and more importantly, the `GetParameterValue()` method allows to get the parsed value of a parameter, returning a `NithParameterValue` struct.

- **NithParameterValue**: This struct represents the value of a specific NITH sensor parameter. It includes: 
  - The `Base` value, which represents the unfiltered parameter value
  - The `Max` value, not always present, which represents the ceiling, the maximum value that the base can assume
  - If both Base and Max are present, the `Normalized` value will be a normalized percentage of the Base over the Max (from 0 to 100)
  - Normally the data are in `string` format. If they are numeric and can be parsed to double, you can retrieve them usin the -AsDouble methods.

### Port Detector
The `PortDetector` package in the NITHlibrary is designed to faciltate and automate the detection of NITH sensors connected to USB ports. It includes the `NithUSBportDetector` class, which manages the scanning process of USB ports for connected sensors.

This class utilizes the `INithUSBportDetectorBehavior` interface to allow for customizable behavior during the detection phases, such as connecting to a specific sensor or handling error scenarios.

The `NithPortDetectorStatus` enum is used to represent the various states of the detection process, including idle, scanning, finished, and error. The scanning process involves opening each USB port, listening for incoming data that matches a specific pattern, and recording any detected sensors.

- [ ] Future work will be focused on making the automatic connection process more efficient

### Wrappers

The `Wrappers` package contains classes and tools to support the interaction with __NITHwrappers__, which are basically pieces of software that "turn into a NITHsensor" a commercially available sensor (e.g. a webcam, or an eye tracker).

- The `NithWebcamWrapper` subpackage in the NITHlibrary is designed to preprocess data from [NITHwebcamWrapper](https://github.com/LIMUNIMI/NITHwebcamWrapper), specifically for calibrating and normalizing values related to eye and mouth apertures. It supports both automatic continuous calibration, which updates minimum and maximum values based on incoming data, and manual calibration modes, where users set these values themselves.

- [ ] The Wrappers package will be updated in the future with more packages to support more wrappers.

### BehaviorTemplates
The `BehaviorTemplates` package provides some abstract classes intended as templates for implementing specific sensor behaviors in response to specific scenarios or data conditions. 

- `ANithBlinkEventBehavior` offers a structured way to handle eye blink events by tracking the state of the eyes and triggering actions based on defined thresholds for eye closure and opening.
- `ANithErrorToStringBehavior` generates standardized error messages for various NITH errors, in string format. 
- `ANithParametersStringBehavior` converts sensor parameters into a comfortably readable multi-line string format, facilitating easier output in graphical user interfaces. 

These abstract classes are designed to be extended, enabling developers to create customized behaviors that fit specific application needs.

### Additional Tools

The library also includes various utility tools organized in distinct namespaces.

#### Filters
The `Filters` namespace in the NITHlibrary provides a set of filtering mechanisms designed to process arrays and individual double values. It includes interfaces and implementations for various types of filters, such as moving average filters, exponentially decaying filters, and others. These filters are useful for smoothing data, reducing noise, and applying different weighting techniques to enhance the quality and stability of sensor readings. Filters include facilities for smooth single value inputs, points, and arrays.

#### Logging

The `Logging` namespace provides an error logging mechanism for GUI applications, addressing the lack of built-in logging facilities of some frameworks (e.g. WPF). It includes services for logging exceptions to a dedicated log file located in a "Logs" directory within the application's base directory, and it formats log entries to include relevant details such as exception messages and stack traces.

#### Mappers
The `Mappers` namespace provides tools for transforming and remapping sensor data. It includes various components designed for specific mapping tasks:

- **AngleBaseChanger**: This component is useful to perform transformations on angular data, specifically to "specify the zero" in a 3D polar coordinates system. This can be useful for example to specify the center when using a headtracker.

- **SegmentMapper**: This class facilitates the mapping of input values from one range to another (i.e. domain change), making it useful for normalizing data and ensuring consistent value representation across different scales.

- **Velocity Calculators**: The namespace features multiple implementations of velocity calculators, which can transform spatial samples into velocity data (i.e. discrete derivative). The various implementations are based on different methods, e.g. kalman filtering, adaptive kalman, or simple step detection with smoothing. These calculators are designed to estimate velocity measurements accurately, catering to different application requirements.

## How to contribute to the development

NITHlibrary is written in C# and can be opened with Visual Studio or another IDE/editor using the provided *.sln* solution file.

To contribute to the development of NITHtester, follow these steps:

1. Clone this repository to your local machine.
2. Open the *NITHlibrary.sln* solution file with Visual Studio.
3. Make your desired changes or additions to the code.

Feel free to fork this repository!

## License

This project is licensed under the [GNU GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html).
