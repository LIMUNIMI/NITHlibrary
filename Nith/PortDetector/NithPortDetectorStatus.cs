namespace NITHlibrary.Nith.PortDetector;

/// <summary>
/// Enumerates the possible statuses of the USB port detection process.
/// </summary>
public enum NithPortDetectorStatus
{
    /// <summary>
    /// The port detection process is idle.
    /// </summary>
    Idle,

    /// <summary>
    /// The port detection process is currently scanning.
    /// </summary>
    Scanning,

    /// <summary>
    /// The port detection process has finished.
    /// </summary>
    Finished,

    /// <summary>
    /// An error occurred during the port detection process.
    /// </summary>
    Error
}