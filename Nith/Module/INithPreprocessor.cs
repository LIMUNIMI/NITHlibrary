using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Module
{
    /// <summary>
    /// Interface for a Nith Preprocessor. 
    /// A preprocessor modifies the NithData before sending it to the Behaviors. Useful for example for calibration or filtering.
    /// </summary>
    public interface INithPreprocessor
    {
        NithSensorData TransformData(NithSensorData sensorData);
    }
}