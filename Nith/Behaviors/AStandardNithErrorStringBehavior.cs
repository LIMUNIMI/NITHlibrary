using System.Text;
using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;

namespace NITHlibrary.Nith.Behaviors
{
    public abstract class AStandardNithErrorStringBehavior : INithErrorBehavior
    {
        private INithModule nithModule;
        private StringBuilder sb = new StringBuilder();

        protected AStandardNithErrorStringBehavior(INithModule nithModule)
        {
            this.nithModule = nithModule;
        }

        public bool HandleError(NithErrors error)
        {
            string errStr = "";

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
                    foreach (string name in nithModule.ExpectedSensorNames)
                    {
                        sb.Clear();
                        errStr += sb.Append("[").Append(name).Append("] ").ToString();
                    }
                    break;
                case NithErrors.Version:
                    errStr = "Error: wrong sensor version connected. Compatible versions are: ";
                    foreach (string version in nithModule.ExpectedVersions)
                    {
                        sb.Clear();
                        errStr += sb.Append("[").Append(version.ToString()).Append("] ").ToString();
                    }
                    break;
                case NithErrors.StatusCode:
                    errStr = "Error: sensor sent an ERR status code (possible hardware error)";
                    break;
                case NithErrors.Values:
                    errStr = "Error: the connected sensor does not provide the required arguments and values. Expected values are: ";
                    foreach (NithParameters argument in nithModule.ExpectedArguments)
                    {
                        sb.Clear();
                        errStr += sb.Append("[").Append(argument.ToString()).Append("] ").ToString();
                    }
                    break;
                case NithErrors.OK:
                    errStr = "Sensor is operating normally!";
                    break;
            }

            ErrorActions(errStr, error);
            return true;
        }

        protected abstract void ErrorActions(string errorString, NithErrors error);
    }
}

