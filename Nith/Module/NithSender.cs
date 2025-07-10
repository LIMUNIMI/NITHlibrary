using System;
using System.Collections.Generic;
using NITHlibrary.Tools.Ports;
using NITHlibrary.Tools.Timers;

namespace NITHlibrary.Tools.Senders
{
    /// <summary>
    /// Sends data to a list of port listeners either on demand or at regular intervals.
    /// </summary>
    public class NithSender
    {
        /// <summary>
        /// Gets or sets the list of port listeners.
        /// </summary>
        public List<IPortSender> PortListeners { get; set; } = new List<IPortSender>();

        private readonly MicroTimer _timer;
        private bool _isPollingEnabled;
        public string DataToSend { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NithSender"/> class.
        /// </summary>
        /// <param name="pollingIntervalInMicroseconds">The interval in microseconds for sending data periodically.</param>
        public NithSender(long pollingIntervalInMicroseconds)
        {
            PortListeners = new List<IPortSender>();
            _timer = new MicroTimer(pollingIntervalInMicroseconds);
            _timer.MicroTimerElapsed += Timer_MicroTimerElapsed;
            _isPollingEnabled = false;
        }

        /// <summary>
        /// Adds a port listener to the sender.
        /// </summary>
        /// <param name="portListener">The port listener to add.</param>
        public void AddPortListener(IPortSender portListener)
        {
            if (portListener == null)
            {
                throw new ArgumentNullException(nameof(portListener));
            }
            PortListeners.Add(portListener);
        }

        /// <summary>
        /// Removes a port listener from the sender.
        /// </summary>
        /// <param name="portListener">The port listener to remove.</param>
        public void RemovePortListener(IPortSender portListener)
        {
            PortListeners.Remove(portListener);
        }

        /// <summary>
        /// Sends data to all registered port listeners.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ObjectDisposedException">Thrown when attempting to use a disposed object.</exception>
        public void SendData(string message)
        {
            foreach (var listener in PortListeners)
            {
                listener.SendMessage(message);
            }
        }

        /// <summary>
        /// Starts the polling timer.
        /// </summary>
        public void StartPolling()
        {
            if (!_isPollingEnabled)
            {
                _timer.Enabled = true;
                _isPollingEnabled = true;
            }
        }

        /// <summary>
        /// Stops the polling timer.
        /// </summary>
        public void StopPolling()
        {
            if (_isPollingEnabled)
            {
                _timer.Enabled = false;
                _isPollingEnabled = false;
            }
        }

        /// <summary>
        /// Handles the MicroTimer elapsed event to send data.
        /// </summary>
        private void Timer_MicroTimerElapsed(object sender, MicroTimerEventArgs e)
        {
            // Here we can choose to send some predefined data or use the last sent data/message
            // For example, we could store the last sent message in a field and resend it
            // For simplicity, we'll send a default message
            SendData(DataToSend);
        }
    }
}
