using NITHlibrary.Nith.Internals;
using NITHlibrary.Tools.Ports;

namespace NITHlibrary.Nith.Module
{
    /// <summary>
    /// A module capable to handle any NITH sensor
    /// </summary>
    public class NithModule : INithModule, IDisposable, IPortListener
    {
        private readonly char[] LINE_DELIM_SYMBOL = new char[] { '$' };

        /// <summary>
        /// Initializes a Nith sensor module, and automatically connects it to a port manager.
        /// </summary>
        public NithModule()
        {
            SensorBehaviors = new List<INithSensorBehavior>();
            ErrorBehaviors = new List<INithErrorBehavior>();
            LastSensorData = new NithSensorData();
            LastError = NithErrors.NaE;
        }

        public List<INithErrorBehavior> ErrorBehaviors { get; protected set; }
        public List<NithParameters> ExpectedArguments { get; set; } = new List<NithParameters>();
        public List<string> ExpectedSensorNames { get; set; } = new List<string>();
        public List<string> ExpectedVersions { get; set; } = new List<string>();
        public NithErrors LastError { get; protected set; }
        public NithSensorData LastSensorData { get; protected set; }
        public List<INithSensorBehavior> SensorBehaviors { get; protected set; }
        public List<INithPreprocessor?> Preprocessors { get; protected set; } = new List<INithPreprocessor?>();

        public void Dispose()
        {
            ErrorBehaviors.Clear();
            SensorBehaviors.Clear();
        }

        void IPortListener.ReceivePortData(string line)
        {
            NithSensorData data = new NithSensorData();
            NithErrors error = NithErrors.NaE;

            try
            {
                data.RawLine = line;

                if (line.StartsWith(LINE_DELIM_SYMBOL[0].ToString()))
                {
                    error = NithErrors.OK; // Set to ok, then check if wrong
                    try
                    {
                        // Check for extra data field
                        string extraData = "";
                        if (line.Split(LINE_DELIM_SYMBOL).Length > 2)
                        {
                            extraData = line.Split(LINE_DELIM_SYMBOL)[2];
                        }

                        // Output splitting
                        string standardLine = line.Split(LINE_DELIM_SYMBOL)[1];

                        string[] fields = standardLine.Split('|');
                        string[] firstField = fields[0].Split('-');
                        string[] arguments = fields[2].Split('&');

                        // Parsings
                        data.ExtraData = extraData;
                        data.SensorName = firstField[0];
                        data.Version = firstField[1];
                        data.StatusCode = NithParsers.ParseStatusCode(fields[1]);
                        foreach (string v in arguments)
                        {
                            string[] s = v.Split('=');
                            string argumentName = s[0];
                            NithArgumentValue value = new NithArgumentValue(NithParsers.ParseField(argumentName), s[1]);

                            data.Values.Add(value);
                        }
                    }
                    catch
                    {
                        error = NithErrors.OutputCompliance;
                    }

                    // Further error checking

                    // Check name
                    if (ExpectedSensorNames.Contains(data.SensorName) || ExpectedSensorNames.Count == 0)
                    {
                        // Check version
                        if (ExpectedVersions.Contains(data.Version) || ExpectedVersions.Count == 0)
                        {
                            // Check status code
                            if (data.StatusCode != NithStatusCodes.ERR)
                            {
                                // Check arguments
                                if (ExpectedArguments.Count != 0 && !data.ContainsParameters(ExpectedArguments))
                                {
                                    error = NithErrors.Values;
                                }
                            }
                            else
                            {
                                error = NithErrors.StatusCode;
                            }
                        }
                        else
                        {
                            error = NithErrors.Version;
                        }
                    }
                    else
                    {
                        error = NithErrors.Name;
                    }
                }
                else
                {
                    error = NithErrors.OutputCompliance;
                }
            }
            catch
            {
                error = NithErrors.Connection;
            }

            // Transform data using preprocessors
            foreach (INithPreprocessor? preProc in Preprocessors)
            {
                data = preProc.TransformData(data);
            }

            // Send to errorbehaviors
            foreach (INithErrorBehavior ebeh in ErrorBehaviors)
            {
                ebeh.HandleError(error);
            }

            // Checks and parsing done! Send to sensorbehaviors
            foreach (INithSensorBehavior sbeh in SensorBehaviors)
            {
                sbeh.HandleData(data);
            }

            LastSensorData = data;
            LastError = error;
        }
    }
}