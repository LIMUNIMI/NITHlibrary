namespace NITHlibrary.Nith.Wrappers.NithWebcamWrapper
{
    /// <summary>
    /// Enum representing the various calibration modes for the NithFaceCam.
    /// </summary>
    public enum NithWebcamCalibrationModes
    {
        /// <summary>
        /// With this mode, the calibration is done automatically. The maximum and the minimum values are continuously updated, deducing them from the incoming data.
        /// </summary>
        AutomaticContinuous,

        /// <summary>
        /// With this mode, the calibration is performed manually: the maximum and the minimum values need to be set by the user.
        /// </summary>
        Manual
    }
}