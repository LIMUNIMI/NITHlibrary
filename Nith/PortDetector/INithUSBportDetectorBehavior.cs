namespace NITHlibrary.Nith.PortDetector
{
    /// <summary>
    /// Interface for a USBportDetectorBehavior, which will specify what to do when the port detection process is finished.
    /// </summary>
    public interface INithUSBportDetectorBehavior
    {
        /// <summary>
        /// Processes the information about the current USB port detection status and associated ports and sensors.
        /// </summary>
        /// <param name="status">The current status of the port detection process.</param>
        /// <param name="portsAndSensors">A dictionary containing port names and associated sensor identifiers.</param>
        void ProcessInformation(NithPortDetectorStatus status, Dictionary<string, string> portsAndSensors);
    }
}