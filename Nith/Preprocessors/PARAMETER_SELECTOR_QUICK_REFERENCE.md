# NithPreprocessor_ParameterSelector - Quick Reference

## Basic Setup (3 Steps)

```csharp
// 1. Create selector
var selector = new NithPreprocessor_ParameterSelector();

// 2. Add rules
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);

// 3. Add to module (as first preprocessor!)
module.Preprocessors.Add(selector);
```

## Modes

```csharp
// Whitelist (default) - only allowed parameters pass
selector.Mode = NithPreprocessor_ParameterSelector.FilterMode.Whitelist;

// Blacklist - all parameters pass except denied ones
selector.Mode = NithPreprocessor_ParameterSelector.FilterMode.Blacklist;
```

## Adding Rules

```csharp
// Single rule
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);

// Multiple rules
selector.AddRules("NITHphone",
    NithParameters.head_acc_yaw,
    NithParameters.head_acc_pitch,
    NithParameters.head_acc_roll);

// Accept ALL from sensor
selector.AddSensorWildcard("NITHphone");
```

## Managing Rules

```csharp
// Clear rules for one sensor
selector.ClearRulesForSensor("NITHphone");

// Clear all rules
selector.ClearAllRules();

// Get rule count
int count = selector.RuleCount;

// Print summary
Console.WriteLine(selector.GetRulesSummary());
```

## Complete Example

```csharp
// Create unified module
var unified = new NithModule();

// Configure selector
var selector = new NithPreprocessor_ParameterSelector();
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);
selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);

// Add to module (FIRST!)
unified.Preprocessors.Add(selector);
unified.Preprocessors.Add(new NithPreprocessor_HeadTrackerCalibrator());

// Connect sources
udpPhone.Listeners.Add(unified);
udpWebcam.Listeners.Add(unified);
udpEye.Listeners.Add(unified);

// Add behavior
unified.SensorBehaviors.Add(new NITHbehavior_HeadViolinBow());
```

## Important Notes

?? **Sensor name only** - No version! Use `"NITHphone"` not `"NITHphone-v1.0"`  
?? **Add as FIRST preprocessor** - Filter before other transformations  
?? **No rules = pass through** - All data passes if no rules configured  
?? **Check for nulls** - Parameters might not be present after filtering  

## Common Patterns

### Pattern 1: Best Source Per Parameter
```csharp
selector.AddRule("NITHphone", NithParameters.head_acc_yaw);      // Fast motion
selector.AddRule("NITHwebcam", NithParameters.mouth_ape);        // Visual
selector.AddRule("NITHeyeTracker", NithParameters.gaze_x);       // Precision
```

### Pattern 2: Block Unwanted Data
```csharp
selector.Mode = NithPreprocessor_ParameterSelector.FilterMode.Blacklist;
selector.AddRule("NITHeyeTracker", NithParameters.head_pos_pitch); // Block
// Everything else passes through
```

### Pattern 3: Trust One Sensor Completely
```csharp
selector.AddSensorWildcard("NITHphone");  // Accept ALL from phone
selector.AddRule("NITHwebcam", NithParameters.mouth_ape); // Only mouth from webcam
```

## See Also

- `PARAMETER_SELECTOR_README.md` - Full API documentation
- `PARAMETER_SELECTOR_EXAMPLES.md` - Comprehensive examples
- `UnifiedSensorSetupExample.cs` - Complete working example
