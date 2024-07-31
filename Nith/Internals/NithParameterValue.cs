using System.Globalization;

namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// Defines the type of parameter. OnlyBase means that the value contains only the base value, while BaseAndMax means that the value contains both the base and max values. Arguments are always in the forms of "base" or "base/max".
    /// </summary>
    public enum NithDataTypes
    {
        OnlyBase,
        BaseAndMax
    }

    /// <summary>
    /// A struct which represents the value of a given NITH sensor parameter. NITH parameters come in the form "parameter=base/max". Here they're unpacked for simplicity.
    /// </summary>
    public struct NithParameterValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NithParameterValue"/> struct, with a parameter and a value in string format.
        /// </summary>
        /// <param name="parameter">The parameter associated with the value.</param>
        /// <param name="baseAndMax">The value in string format, which may contain a base value and optional max value, in the form "base/max"</param>
        public NithParameterValue(NithParameters parameter, string baseAndMax)
        {
            string[] chunks = baseAndMax.Split('/');
            if (chunks.Length == 1) // Only Value case
            {
                Type = NithDataTypes.OnlyBase;
                Base = chunks[0];
                Max = "";
                Normalized = double.NaN;
            }
            else // Value and Max case
            {
                Type = NithDataTypes.BaseAndMax;
                Base = chunks[0];
                Max = chunks[1].Replace("\n", "");
                Normalized = double.Parse(Base, CultureInfo.InvariantCulture) * 100 / double.Parse(Max, CultureInfo.InvariantCulture);
            }

            Parameter = parameter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NithParameterValue"/> struct with a parameter, base value, and max value.
        /// </summary>
        /// <param name="parameter">The parameter associated with the value.</param>
        /// <param name="base">The base value as a string.</param>
        /// <param name="max">The max value as a string.</param>
        public NithParameterValue(NithParameters parameter, string @base, string max)
        {
            Type = NithDataTypes.BaseAndMax;
            this.Base = @base;
            this.Max = max.Replace("\n", "");
            Normalized = double.Parse(@base, CultureInfo.InvariantCulture) * 100 / double.Parse(max, CultureInfo.InvariantCulture);

            Parameter = parameter;
        }

        /// <summary>
        /// Gets or sets the parameter associated with this <see cref="NithParameterValue"/>.
        /// </summary>
        public NithParameters Parameter { get; set; }

        /// <summary>
        /// Gets or sets the base value as a string.
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Gets the base value as a boolean.
        /// </summary>
        public bool BaseAsBool
        {
            get
            {
                try
                {
                    return bool.Parse(Base);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the base value as a double.
        /// </summary>
        public double BaseAsDouble
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

        /// <summary>
        /// Gets or sets the max value as a string.
        /// </summary>
        public string Max { get; set; }

        /// <summary>
        /// Gets the max value as a double.
        /// </summary>
        public double MaxAsDouble
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

        /// <summary>
        /// Gets or sets the normalized value, from 0 to 100, taking into account the Max value. It's calculated as (Base / Max) * 100.
        /// </summary>
        public double Normalized { get; set; }

        /// <summary>
        /// Gets or sets the type of data (OnlyBase or BaseAndMax).
        /// </summary>
        public NithDataTypes Type { get; set; }
    }
}
