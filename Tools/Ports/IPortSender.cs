using System;

namespace NITHlibrary.Tools.Ports
{
    /// <summary>
    /// Defines methods and properties for sending messages via ports.
    /// </summary>
    public interface IPortSender : IDisposable
    {
        /// <summary>
        /// Gets or sets the port number to send messages to.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Sends a message to the specified port.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ObjectDisposedException">Thrown when attempting to use a disposed object.</exception>
        void SendMessage(string message);
    }
}
