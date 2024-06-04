namespace NITHlibrary.Nith.Internals
{
    public class NithSensorData
    {
        public string RawLine { get; set; } = string.Empty;
        public string SensorName { get; set; }
        public string Version { get; set; }
        public NithStatusCodes StatusCode { get; set; }
        public List<NithArgumentValue> Values { get; set; }
        public string ExtraData { get; set; }

        public NithSensorData()
        {
            SensorName = "";
            Version = "";
            StatusCode = NithStatusCodes.NaC;
            Values = new List<NithArgumentValue>();
        }

        public void Reset()
        {
            SensorName = "";
            Version = "";
            StatusCode = NithStatusCodes.NaC;
            Values.Clear();
        }
        public NithArgumentValue? GetParameter(NithParameters argument)
        {
            foreach (NithArgumentValue value in Values)
            {
                if (value.Argument == argument)
                {
                    return value;
                }
            }
            return null;
        }

        public bool ContainsParameter(NithParameters argument)
        {
            foreach (NithArgumentValue value in Values)
            {
                if (value.Argument == argument)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsParameters(List<NithParameters> arguments)
        {
            foreach (NithParameters argument in arguments)
            {
                if (!ContainsParameter(argument))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
