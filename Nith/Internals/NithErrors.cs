using NITHlibrary.Nith.Module;

namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// A list of all the possible errors that can be thrown by a <see cref="NithModule"/>.
    /// </summary>
    public enum NithErrors
    {
        /// <summary>
        /// Not an Error
        /// </summary>
        NaE,
        /// <summary>
        /// Connection error
        /// </summary>
        Connection,
        /// <summary>
        /// The sensor output does not comply with NITH standard
        /// </summary>
        OutputCompliance,
        /// <summary>
        /// The sensor name is not on the <see cref="NithModule"/>'s supported sensors name list
        /// </summary>
        Name,
        /// <summary>
        /// The sensor version is not on the <see cref="NithModule"/>'s supported sensors version list
        /// </summary>
        Version,
        /// <summary>
        /// The sensor status code is ERR
        /// </summary>
        StatusCode,
        /// <summary>
        /// The sensor does not provide the required list of <see cref="NithParameters"/>
        /// </summary>
        Parameters,
        /// <summary>
        /// No error
        /// </summary>
        Ok
    }
}