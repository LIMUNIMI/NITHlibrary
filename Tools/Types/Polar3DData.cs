namespace NITHlibrary.Tools.Types
{
    /// <summary>
    /// Represents a set of polar coordinates in a 3D space.
    /// </summary>
    public struct Polar3DData
    {
        /// <summary>
        /// Gets or sets the yaw angle, which represents rotation around the vertical axis.
        /// </summary>
        public double Yaw;

        /// <summary>
        /// Gets or sets the pitch angle, which represents the rotation around the lateral or transverse axis.
        /// </summary>
        public double Pitch;

        /// <summary>
        /// Gets or sets the roll angle, which represents the rotation around the longitudinal axis.
        /// </summary>
        public double Roll;
    }
}