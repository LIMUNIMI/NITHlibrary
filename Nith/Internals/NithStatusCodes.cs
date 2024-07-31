namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// Enum representing various status codes from a NITH sensor.
    /// </summary>
    public enum NithStatusCodes
    {
        /// <summary>
        /// Operative status.
        /// </summary>
        OPR,   

        /// <summary>
        /// Generic error status.
        /// </summary>
        ERR,    

        /// <summary>
        /// Status indicating the need for calibration.
        /// </summary>
        NCA,    

        /// <summary>
        /// Status indicating that calibration is in progress.
        /// </summary>
        ICA,    

        /// <summary>
        /// Indicates that the value is not a recognized status code.
        /// </summary>
        NaC     
    }
}