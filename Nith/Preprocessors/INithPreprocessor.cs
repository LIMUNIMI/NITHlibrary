using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// Interface for a Nith Preprocessor. 
    /// A preprocessor modifies the incoming <see cref="NithSensorData"/> before sending it to the Behaviors. Useful for example for post-calibration or data filtering.
    /// </summary>
    public interface INithPreprocessor
    {
        /// <summary>
        /// Transforms the <see cref="NithSensorData"/> before sending it to the Behaviors./>
        /// </summary>
        /// <param name="sensorData"></param>
        /// <returns></returns>
        NithSensorData TransformData(NithSensorData sensorData);
    }
}