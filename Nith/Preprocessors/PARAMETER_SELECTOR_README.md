# NithPreprocessor_ParameterSelector

## Overview

`NithPreprocessor_ParameterSelector` is a powerful preprocessor that enables **NITH sensor data multiplexing** by selectively filtering parameters based on their source sensor. This allows you to combine data from multiple NITH sensors into a single unified data stream.

## Use Cases

- **Combine multiple sensors**: Accept head tracking from phone, mouth aperture from webcam, and gaze from eye tracker - all in one module
- **Choose best source per parameter**: Use phone for acceleration, webcam for mouth, eye tracker for gaze
- **Avoid parameter conflicts**: When multiple sensors provide the same parameter (e.g., head position), choose which source to trust
- **Sensor fusion**: Combine complementary data from different sources for better accuracy

## Key Features

? **Whitelist or Blacklist mode** - Accept specific parameters or block specific parameters  
? **Per-sensor configuration** - Different rules for each sensor  
? **Wildcard support** - Accept/reject ALL parameters from a sensor  
? **Sensor name only** - No version matching needed  
? **Zero-copy filtering** - Lightweight and fast  
? **Dynamic reconfiguration** - Change rules at runtime  
? **Debugging support** - Print rule summary for troubleshooting  

## Quick Start

### Basic Whitelist Example

```csharp
// Create a unified module
var unifiedModule = new NithModule();

// Create and configure the selector
var selector = new NithPreprocessor_ParameterSelector();
// Default mode is Whitelist

// Define which parameters to accept from each sensor
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
selector.AddRule("NITHphone", NithParameters.head_pos_pitch);

selector.AddRule("NITHwebcam", NithParameters.mouth_ape);
selector.AddRule("NITHwebcam", NithParameters.eyeLeft_ape);

selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_y);

// Add to module (as first preprocessor!)
unifiedModule.Preprocessors.Add(selector);

// Connect multiple data sources
udpReceiverPhone.Listeners.Add(unifiedModule);
udpReceiverWebcam.Listeners.Add(unifiedModule);
udpReceiverEyeTracker.Listeners.Add(unifiedModule);
```

### Basic Blacklist Example

```csharp
var selector = new NithPreprocessor_ParameterSelector();
selector.Mode = NithPreprocessor_ParameterSelector.FilterMode.Blacklist;

// Block specific parameters from specific sensors
// (everything else passes through)
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_pitch);
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_roll);

unifiedModule.Preprocessors.Add(selector);
```

## API Reference

### Methods

#### `AddRule(string sensorName, NithParameters parameter)`
Adds a rule for a specific parameter from a specific sensor.

- **sensorName**: Sensor name WITHOUT version (e.g., `"NITHphone"`, not `"NITHphone-v1.0"`)
- **parameter**: The parameter to filter

**Whitelist mode**: Parameter will be accepted  
**Blacklist mode**: Parameter will be rejected

#### `AddRules(string sensorName, params NithParameters[] parameters)`
Adds multiple rules at once for a sensor.

```csharp
selector.AddRules("NITHphone",
    NithParameters.head_acc_yaw,
    NithParameters.head_acc_pitch,
    NithParameters.head_acc_roll
);
```

#### `AddSensorWildcard(string sensorName)`
Accept/reject ALL parameters from a sensor.

**Whitelist mode**: All parameters from this sensor will pass  
**Blacklist mode**: All parameters from this sensor will be blocked

```csharp
selector.AddSensorWildcard("NITHphone"); // Accept everything from phone
```

#### `ClearRulesForSensor(string sensorName)`
Removes all rules for a specific sensor.

#### `ClearAllRules()`
Removes all configured rules.

#### `GetRulesSummary()`
Returns a string describing all configured rules (useful for debugging).

```csharp
Console.WriteLine(selector.GetRulesSummary());
// Output:
// NithPreprocessor_ParameterSelector (Mode: Whitelist)
//   NITHphone: Accept 3 parameter(s): head_acc_yaw, head_acc_pitch, head_acc_roll
//   NITHwebcam: Accept 2 parameter(s): mouth_ape, eyeLeft_ape
```

### Properties

#### `Mode` (get/set)
Sets the filtering mode.

- `FilterMode.Whitelist` (default): Only explicitly allowed parameters pass
- `FilterMode.Blacklist`: All parameters pass except explicitly denied ones

#### `RuleCount` (get)
Returns the number of sensors with configured rules.

## How It Works

1. **Receives `NithSensorData`** from multiple sources (UDP/USB receivers)
2. **Checks sensor name** against configured rules
3. **Filters parameters** based on mode and rules
4. **Returns modified `NithSensorData`** with only allowed parameters

### Whitelist Mode Logic
```
For each parameter in incoming data:
  Does sensor have rules? 
    No  ? REJECT (default deny)
    Yes ? Is this parameter in the allowed list?
            Yes ? ACCEPT
            No  ? REJECT
```

### Blacklist Mode Logic
```
For each parameter in incoming data:
  Does sensor have rules?
    No  ? ACCEPT (default allow)
    Yes ? Is this parameter in the denied list?
            Yes ? REJECT
            No  ? ACCEPT
```

## Important Notes

### ?? Sensor Name Matching
- Rules match against **`SensorName`** field only
- Version is **NOT** included
- ? Correct: `"NITHphone"`
- ? Incorrect: `"NITHphone-v1.0"`

### ?? Preprocessor Order
Add the `ParameterSelector` as the **first preprocessor** if you want to filter before other transformations:

```csharp
// ? Correct order
module.Preprocessors.Add(parameterSelector);     // Filter first
module.Preprocessors.Add(calibrator);            // Then calibrate
module.Preprocessors.Add(smoothingFilter);       // Then smooth

// ? Wrong order (will smooth parameters that get filtered out)
module.Preprocessors.Add(smoothingFilter);       
module.Preprocessors.Add(parameterSelector);     
```

### ?? No Rules = Pass Through
If no rules are configured, **all parameters pass through unchanged**.

### ?? Empty Values After Filtering
After filtering, `NithSensorData.Values` may be empty or missing expected parameters. Behaviors should handle this gracefully:

```csharp
// Good practice
var pitchValue = nithData.GetParameterValue(NithParameters.head_pos_pitch);
if (pitchValue.HasValue)
{
    // Use the value
}
```

## Performance

- **O(1)** dictionary lookup per parameter
- **No memory allocations** during filtering
- **Negligible overhead** compared to network I/O and parsing
- Suitable for **real-time applications** with high data rates (100+ Hz)

## Examples

See `PARAMETER_SELECTOR_EXAMPLES.md` for comprehensive usage examples including:
- Simple parameter selection
- Sensor wildcards
- Blacklist mode
- Dynamic rule changes
- HeadBower typical setup
- Common patterns (best source, fallback sources, sensor fusion)

## Architecture Comparison

### Before (Multiple Modules)
```
Phone UDP ? NithModulePhone ? Behavior_HeadBow
Webcam UDP ? NithModuleWebcam ? (different behavior)
EyeTracker UDP ? NithModuleEyeTracker ? (different behavior)
```
Problem: Can't combine data from different sensors in one behavior

### After (Unified Module with Selector)
```
Phone UDP ???????
                ???? NithModuleUnified ? ParameterSelector ? Behavior_HeadBow
Webcam UDP ??????                        (filters per source)
                ?
EyeTracker UDP ??
```
Solution: One behavior sees combined data from all sensors!

## License

Part of NITHlibrary - GNU GPLv3

## See Also

- `PARAMETER_SELECTOR_EXAMPLES.md` - Comprehensive usage examples
- `INithPreprocessor` - Base interface for all preprocessors
- `NithModule` - Main NITH data processing module
- `NithSensorData` - Data structure containing sensor parameters
