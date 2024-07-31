namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// An interface which defines a NITH sensor behavior.
    /// </summary>
    public interface INithSensorBehavior
    {
        /// <summary>
        /// Defines how to handle incoming sensor data.
        /// </summary>
        /// <param name="nithData">Sensor data</param>
        void HandleData(NithSensorData nithData);
    }
}