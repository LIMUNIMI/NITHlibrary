using System.Text;
using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;

namespace NITHlibrary.Nith.Behaviors
{
    /// <summary>
    /// A metabehavior which creates a standard string for each of the possible NITH errors.
    /// </summary>
    public abstract class ANithErrorToStringBehavior : INithErrorBehavior
    {
        private readonly NithModule _nithModule;
        private readonly StringBuilder _sb = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ANithErrorToStringBehavior"/> class.
        /// </summary>
        /// <param name="nithModule">The NITH module instance associated with this behavior. Needed to extract required parameters.</param>
        protected ANithErrorToStringBehavior(NithModule nithModule)
        {
            _nithModule = nithModule;
        }

        /// <summary>
        /// Handles the specified error and generates a standard error string.
        /// </summary>
        /// <param name="error">The error to handle.</param>
        /// <returns>True if an error was handled, false otherwise.</returns>
        public bool HandleError(NithErrors error)
        {
            var errStr = "";

            switch (error)
            {
                case NithErrors.NaE:
                    return false;
                case NithErrors.Connection:
                    errStr = "Error: no connection to any sensor on the selected port";
                    break;
                case NithErrors.OutputCompliance:
                    errStr = "Error: the connected sensor was not recognized as a NITH sensor (input format does not comply with the standard)";
                    break;
                case NithErrors.Name:
                    errStr = "Error: wrong sensor name or model connected. Compatible sensors are: ";
                    foreach (var name in _nithModule.ExpectedSensorNames)
                    {
                        _sb.Clear();
                        errStr += _sb.Append("[").Append(name).Append("] ").ToString();
                    }
                    break;
                case NithErrors.Version:
                    errStr = "Error: wrong sensor version connected. Compatible versions are: ";
                    foreach (var version in _nithModule.ExpectedVersions)
                    {
                        _sb.Clear();
                        errStr += _sb.Append("[").Append(version.ToString()).Append("] ").ToString();
                    }
                    break;
                case NithErrors.StatusCode:
                    errStr = "Error: sensor sent an ERR status code (possible hardware error)";
                    break;
                case NithErrors.Parameters:
                    errStr = "Error: the connected sensor does not provide the required parameters. Expected parameters are: ";
                    foreach (var argument in _nithModule.ExpectedParameters)
                    {
                        _sb.Clear();
                        errStr += _sb.Append("[").Append(argument.ToString()).Append("] ").ToString();
                    }
                    break;
                case NithErrors.Ok:
                    errStr = "Sensor is operating normally!";
                    break;
            }

            ErrorActions(errStr, error);
            return true;
        }

        /// <summary>
        /// Defines the actions to take when an error occurs. This method should be implemented by derived classes to define specific behavior.
        /// </summary>
        /// <param name="errorString">The standard error string associated with that error.</param>
        /// <param name="error">The error type.</param>
        protected abstract void ErrorActions(string errorString, NithErrors error);
    }
}
