namespace NITHlibrary.Nith.Internals
{
    public class NithErrorData
    {
        public NithErrorData()
        {
            IsConnectionOk = false;
            IsSensorNith = false;
            IsSensorNameOk = false;
            IsSensorVersionOk = false;
            IsStatusCodeOk = false;
            IsValuesOk = false;
        }

        public bool IsConnectionOk { get; set; }
        public bool IsSensorNith { get; set; }
        public bool IsSensorNameOk { get; set; }
        public bool IsSensorVersionOk { get; set; }
        public bool IsStatusCodeOk { get; set; }
        public bool IsValuesOk { get; set; }

        public void Reset()
        {
            IsConnectionOk = false;
            IsSensorNith = false;
            IsSensorNameOk = false;
            IsSensorVersionOk = false;
            IsStatusCodeOk = false;
            IsValuesOk = false;
        }

        public bool IsEverythingOk()
        {
            return IsConnectionOk && IsSensorNith && IsSensorNameOk && IsSensorVersionOk && IsStatusCodeOk && IsValuesOk;
        }
    }

    public enum NithErrors
    {
        NaE,
        Connection,
        OutputCompliance,
        Name,
        Version,
        StatusCode,
        Values,
        OK
    }
}