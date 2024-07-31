using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Behaviors
{
    /// <summary>
    /// A metabehavior which converts NITH parameters into a string that is comfortable to read (e.g., in console output or in a TextBox).
    /// </summary>
    public abstract class ANithParametersStringBehavior : INithSensorBehavior
    {
        private const int MinArgLength = 15;
        private string _argumentStr = "";

        /// <summary>
        /// Handles the incoming sensor data and converts the parameters into a readable string format.
        /// </summary>
        /// <param name="nithData">The sensor data received.</param>
        public void HandleData(NithSensorData nithData)
        {
            _argumentStr = "";
            foreach (var val in nithData.Values)
            {
                _argumentStr += AddWhiteSpaces(val.Parameter.ToString());
                _argumentStr += "v: ";
                if (val.Type == NithDataTypes.OnlyBase)
                {
                    _argumentStr += val.Base;
                }
                else if (val.Type == NithDataTypes.BaseAndMax)
                {
                    _argumentStr += val.Base + " / " + val.Max + "\tp: " + val.Normalized.ToString("F2");
                }

                _argumentStr += "\n";
            }

            // Send for processing
            HandleParametersString(_argumentStr);
        }

        /// <summary>
        /// Adds white spaces to the input string to ensure it meets the minimum parameter length.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The input string padded with white spaces if necessary.</returns>
        private string AddWhiteSpaces(string input)
        {
            if (input.Length < MinArgLength)
            {
                var numberOfSpaces = MinArgLength - input.Length;
                string whiteSpaces = new(' ', numberOfSpaces);
                return input + whiteSpaces;
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// Handles the formatted parameters string. This method should be implemented by derived classes to define specific behavior.
        /// </summary>
        /// <param name="parametersString">The formatted parameters string.</param>
        protected abstract void HandleParametersString(string parametersString);
    }
}
