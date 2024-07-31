namespace NITHlibrary.Tools.Ports
{
    /// <summary>
    /// Defines a port receiver listener.
    /// </summary>
    public interface IPortListener
    {
        /// <summary>
        /// Method to process the data received from the port.
        /// </summary>
        /// <param name="line">The data line received from the port.</param>
        void ReceivePortData(string line);
    }
}