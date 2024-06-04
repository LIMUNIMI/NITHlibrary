namespace NITHlibrary.Nith.PortDetector
{
    public interface INithUSBportDetectorBehavior
    {
        void ProcessInformation(NithPortDetectorInfo status, Dictionary<string, string> portsAndSensors);
    }

    public enum NithPortDetectorInfo
    {
        Idle,
        Scanning,
        Finished,
        Error
    }
}