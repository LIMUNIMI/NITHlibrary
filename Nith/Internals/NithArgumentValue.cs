using System.Globalization;

namespace NITHlibrary.Nith.Internals
{
    public enum NithDataTypes
    {
        OnlyBase,
        BaseAndMax
    }

    public struct NithArgumentValue
    {
        public NithArgumentValue(NithParameters argument, string valueString)
        {
            string[] chunks = valueString.Split('/');
            if (chunks.Length == 1) // Only Value case
            {
                Type = NithDataTypes.OnlyBase;
                Base = chunks[0];
                Max = "";
                Proportional = double.NaN;
            }
            else // Value and Max case
            {
                Type = NithDataTypes.BaseAndMax;
                Base = chunks[0];
                Max = chunks[1].Replace("\n", "");
                Proportional = double.Parse(Base, CultureInfo.InvariantCulture) * 100 / double.Parse(Max, CultureInfo.InvariantCulture);
            }

            Argument = argument;
        }

        public NithArgumentValue(NithParameters argument, string Base, string Max)
        {
            Type = NithDataTypes.BaseAndMax;
            this.Base = Base;
            this.Max = Max.Replace("\n", "");
            Proportional = double.Parse(Base, CultureInfo.InvariantCulture) * 100 / double.Parse(Max, CultureInfo.InvariantCulture);

            Argument = argument;
        }

        public NithParameters Argument { get; set; }
        public string Base { get; set; }
        public double Base_AsDouble
        {
            get
            {
                try
                {
                    return double.Parse(Base, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return double.NaN;
                }
            }
        }
        public string Max { get; set; }
        public double Max_AsDouble
        {
            get
            {
                try
                {
                    return double.Parse(Max, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return double.NaN;
                }
            }
        }
        public double Proportional { get; set; }
        public NithDataTypes Type { get; set; }
    }
}