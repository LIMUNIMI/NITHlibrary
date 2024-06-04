namespace NITHlibrary.Nith.Internals
{
    internal static class NithParsers
    {
        public static NithStatusCodes ParseStatusCode(string code)
        {
            NithStatusCodes ret = NithStatusCodes.NaC;
            try
            {
                ret = (NithStatusCodes)Enum.Parse(typeof(NithStatusCodes), code);
            }
            catch
            {

            }
            return ret;
        }

        public static NithParameters ParseField(string field)
        {
            NithParameters ret = NithParameters.NaA;
            try
            {
                ret = (NithParameters)Enum.Parse(typeof(NithParameters), field);
            }
            catch
            {

            }
            return ret;
        }
    }
}
