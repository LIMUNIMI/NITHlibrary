# NithPreprocessor_ParameterSelector - Usage Examples

## Overview

The `NithPreprocessor_ParameterSelector` allows you to multiplex NITH sensor data from multiple sources by selectively accepting or rejecting specific parameters based on their source sensor.

This is useful when:
- Multiple sensors provide overlapping parameters (e.g., both phone and eye tracker provide head position)
- You want to use the best source for each parameter
- You need to combine data from multiple sensors into a single unified stream

## Basic Concepts

### Whitelist Mode (Default)
- **Only explicitly allowed parameters pass through**
- All other parameters are filtered out
- Use when you want strict control over which data is accepted

### Blacklist Mode
- **All parameters pass through except those explicitly denied**
- Use when you want to block specific parameters from specific sources

## Usage Examples

### Example 1: Simple Parameter Selection (Whitelist)

```csharp
// Create a unified module that receives data from multiple sources
var unifiedModule = new NithModule();

// Create the parameter selector
var selector = new NithPreprocessor_ParameterSelector();
// Default mode is Whitelist

// Define rules: Accept specific parameters only from designated sensors
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
selector.AddRule("NITHphone", NithParameters.head_pos_pitch);
selector.AddRule("NITHphone", NithParameters.head_pos_roll);

selector.AddRule("NITHbreath", NithParameters.mouth_ape);
selector.AddRule("NITHbreath", NithParameters.breath_intensity);

selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_y);
selector.AddRule("NITHeyeTracker", NithParameters.left_eye_ape);
selector.AddRule("NITHeyeTracker", NithParameters.right_eye_ape);

// Add the selector to the module's preprocessor chain
unifiedModule.Preprocessors.Add(selector);

// Connect multiple receivers to this unified module
udpReceiverPhone.Listeners.Add(unifiedModule);
udpReceiverBreath.Listeners.Add(unifiedModule);
udpReceiverEyeTracker.Listeners.Add(unifiedModule);

// Now the unified module will receive:
// - head_acc_yaw, head_pos_pitch, head_pos_roll from phone ONLY
// - mouth_ape, breath_intensity from breath sensor ONLY
// - gaze_x, gaze_y, left_eye_ape, right_eye_ape from eye tracker ONLY
```

### Example 2: Accept All Parameters from One Sensor

```csharp
var selector = new NithPreprocessor_ParameterSelector();

// Accept ALL parameters from the phone
selector.AddSensorWildcard("NITHphone");

// Accept only specific parameters from other sensors
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_y);

unifiedModule.Preprocessors.Add(selector);
```

### Example 3: Blacklist Mode - Block Specific Parameters

```csharp
var selector = new NithPreprocessor_ParameterSelector();
selector.Mode = NithPreprocessor_ParameterSelector.FilterMode.Blacklist;

// Block head position from eye tracker (prefer phone's head tracking)
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_pitch);
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_roll);
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_yaw);

// Accept everything else from all sensors
unifiedModule.Preprocessors.Add(selector);
```

### Example 4: Multiple Rules with AddRules Helper

```csharp
var selector = new NithPreprocessor_ParameterSelector();

// Add multiple rules at once
selector.AddRules("NITHphone",
    NithParameters.head_acc_yaw,
    NithParameters.head_acc_pitch,
    NithParameters.head_acc_roll,
    NithParameters.head_pos_pitch,
    NithParameters.head_pos_roll
);

selector.AddRules("NITHwebcam",
    NithParameters.mouth_ape,
    NithParameters.left_eye_ape,
    NithParameters.right_eye_ape
);

unifiedModule.Preprocessors.Add(selector);
```

### Example 5: HeadBower Typical Setup

```csharp
// In DefaultSetup.cs

// Create a unified head tracking module
Rack.NithModuleUnifiedHeadTracking = new NithModule();
Rack.NithModuleUnifiedHeadTracking.MaxQueueSize = 20;
Rack.NithModuleUnifiedHeadTracking.OverflowBehavior = QueueOverflowBehavior.DropOldest;

// Create parameter selector
var headTrackingSelector = new NithPreprocessor_ParameterSelector();

// Phone provides: acceleration and head position
headTrackingSelector.AddRules("NITHphone",
    NithParameters.head_acc_yaw,
    NithParameters.head_acc_pitch,
    NithParameters.head_acc_roll,
    NithParameters.head_pos_pitch,
    NithParameters.head_pos_roll
);

// Webcam provides: mouth and eye apertures
headTrackingSelector.AddRules("NITHwebcam",
    NithParameters.mouth_ape,
    NithParameters.left_eye_ape,
    NithParameters.right_eye_ape
);

// Eye tracker provides: gaze data
headTrackingSelector.AddRules("NITHeyeTracker",
    NithParameters.gaze_x,
    NithParameters.gaze_y
);

// Add selector as first preprocessor
Rack.NithModuleUnifiedHeadTracking.Preprocessors.Add(headTrackingSelector);

// Then add other preprocessors (calibrators, filters, etc.)
Rack.NithModuleUnifiedHeadTracking.Preprocessors.Add(new NithPreprocessor_HeadTrackerCalibrator());
Rack.NithModuleUnifiedHeadTracking.Preprocessors.Add(new NithPreprocessor_HeadAccelerationCalculator());

// Connect all data sources to the unified module
Rack.UDPreceiverPhone.Listeners.Add(Rack.NithModuleUnifiedHeadTracking);
Rack.UDPreceiverWebcam.Listeners.Add(Rack.NithModuleUnifiedHeadTracking);
Rack.UDPreceiverEyeTracker.Listeners.Add(Rack.NithModuleUnifiedHeadTracking);

// Add your behavior
Rack.NithModuleUnifiedHeadTracking.SensorBehaviors.Add(Rack.Behavior_HeadBow);
```

### Example 6: Dynamic Rule Changes

```csharp
var selector = new NithPreprocessor_ParameterSelector();

// Initial setup
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);

// Later, switch to webcam for head tracking
selector.ClearRulesForSensor("NITHphone");
selector.AddRule("NITHwebcam", NithParameters.head_pos_yaw);

// Or clear everything and reconfigure
selector.ClearAllRules();
selector.AddRules("NITHphone", 
    NithParameters.head_acc_yaw,
    NithParameters.head_acc_pitch
);
```

## Debugging and Monitoring

```csharp
// Print configured rules
var selector = new NithPreprocessor_ParameterSelector();
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);

Console.WriteLine(selector.GetRulesSummary());
// Output:
// NithPreprocessor_ParameterSelector (Mode: Whitelist)
//   NITHphone: Accept 1 parameter(s): head_acc_yaw
//   NITHwebcam: Accept 1 parameter(s): mouth_ape

// Check how many sensors have rules
int count = selector.RuleCount;
Console.WriteLine($"Rules configured for {count} sensor(s)");
```

## Important Notes

1. **Sensor Name Matching**: The selector matches against `SensorName` field in `NithSensorData`, which does NOT include the version number.
   - ? Correct: `"NITHphone"` (without version)
   - ? Incorrect: `"NITHphone-v1.0"` (with version)

2. **Order Matters**: Add the `ParameterSelector` as the **first preprocessor** in the chain if you want to filter before other transformations:
   ```csharp
   // Filter FIRST
   module.Preprocessors.Add(selector);
   // Then calibrate
   module.Preprocessors.Add(calibrator);
   // Then filter values
   module.Preprocessors.Add(smoothingFilter);
   ```

3. **No Rules = Pass Through**: If no rules are configured, all parameters pass through unchanged.

4. **Empty Values List**: After filtering, `NithSensorData.Values` will only contain the allowed parameters. Behaviors should handle cases where expected parameters might not be present.

## Performance Considerations

- The selector is very lightweight (O(1) dictionary lookup per parameter)
- No memory allocations during filtering
- Suitable for real-time applications with high data rates
- Negligible performance impact compared to network I/O and parsing

## Common Patterns

### Pattern 1: "Best Source" Selection
Choose the best sensor for each data type:
```csharp
// Use phone for motion (accelerometer)
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
// Use eye tracker for gaze (more accurate)
selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);
// Use webcam for mouth (easier to see)
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);
```

### Pattern 2: "Fallback Sources"
Accept from multiple sources but prefer one (requires custom behavior logic):
```csharp
// Accept head position from both phone and webcam
// Your behavior can prioritize phone if both are present
selector.AddRule("NITHphone", NithParameters.head_pos_pitch);
selector.AddRule("NITHwebcam", NithParameters.head_pos_pitch);
```

### Pattern 3: "Sensor Fusion"
Combine complementary data:
```csharp
// Phone: fast motion detection
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
// Webcam: absolute position (slower but drift-free)
selector.AddRule("NITHwebcam", NithParameters.head_pos_yaw);
// Your behavior can combine both for best results
```
