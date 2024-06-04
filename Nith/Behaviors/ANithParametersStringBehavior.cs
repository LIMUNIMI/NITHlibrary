using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Behaviors
{
    public abstract class ANithParametersStringBehavior : INithSensorBehavior
    {
        private const int MIN_ARG_LENGTH = 15;
        private string argumentStr = "";

        public void HandleData(NithSensorData nithData)
        {
            argumentStr = "";
            foreach (NithArgumentValue val in nithData.Values)
            {
                argumentStr += AddWhiteSpaces(val.Argument.ToString());
                argumentStr += "v: ";
                if (val.Type == NithDataTypes.OnlyBase)
                {
                    argumentStr += val.Base;
                }
                else if (val.Type == NithDataTypes.BaseAndMax)
                {
                    argumentStr += val.Base + " / " + val.Max + "\tp: " + val.Proportional.ToString("F2");
                }

                argumentStr += "\n";
            }

            // Send for processing
            HandleParametersString(argumentStr);
        }

        private string AddWhiteSpaces(string input)
        {
            if (input.Length < MIN_ARG_LENGTH)
            {
                int numberOfSpaces = MIN_ARG_LENGTH - input.Length;
                string whiteSpaces = new string(' ', numberOfSpaces);
                return input + whiteSpaces;
            }
            else
            {
                return input;
            }
        }

        protected abstract void HandleParametersString(string parametersString);
    }
}