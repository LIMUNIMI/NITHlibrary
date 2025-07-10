using System;
using System.Globalization;

namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// Defines the type of parameter value. OnlyValue means that only a single value is provided,
    /// while Range means that a minimum (b:), an actual value (v:) and a maximum (m:) were provided.
    /// </summary>
    public enum NithDataTypes
    {
        OnlyValue,
        Range
    }

    /// <summary>
    /// Represents the value of a given NITH sensor parameter.
    /// In the simple case, a parameter is represented as "parameter=value" (OnlyValue).
    /// In the composite case, the parameter is represented as "parameter=[b:xxx/v:yyy/m:zzz]" where:
    ///   b:    the minimum (previously called "base"),
    ///   v:    the actual value,
    ///   m:    the maximum.
    /// The normalized value is computed as ((v - b) / (m - b)) * 100.
    /// </summary>
    public struct NithParameterValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NithParameterValue"/> struct,
        /// by parsing a raw value string.
        /// </summary>
        /// <param name="parameter">The parameter associated with the value.</param>
        /// <param name="rawValue">The value in string format. It can be either a simple value or in the form "b:xxx/v:yyy/m:zzz".</param>
        public NithParameterValue(NithParameters parameter, string rawValue)
        {
            Parameter = parameter;
            Normalized = double.NaN;
            DataType = NithDataTypes.OnlyValue;
            // Default assignments for fields in case of only simple value.
            Value = rawValue;
            Min = "";
            Max = "";

            // Check if the raw value represents a composite range format, i.e. if it contains parts separated by '/'
            // (The new composite format must contain exactly three tokens in any order: one starting with "b:", one "v:" and one "m:")
            string[] chunks = rawValue.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 1)
            {
                // Simple case
                DataType = NithDataTypes.OnlyValue;
                Value = chunks[0];
            }
            else if (chunks.Length == 3)
            {
                // Composite range case.
                // We expect tokens tagged with "b:" (min), "v:" (value) and "m:" (max).
                string minTmp = null, valueTmp = null, maxTmp = null;
                foreach (var token in chunks)
                {
                    if (token.StartsWith("b:"))
                    {
                        minTmp = token.Substring(2);
                    }
                    else if (token.StartsWith("v:"))
                    {
                        valueTmp = token.Substring(2);
                    }
                    else if (token.StartsWith("m:"))
                    {
                        maxTmp = token.Substring(2);
                    }
                }

                if (string.IsNullOrEmpty(minTmp) || string.IsNullOrEmpty(valueTmp) || string.IsNullOrEmpty(maxTmp))
                {
                    //throw new Exception("Composite range value must include tokens for b:, v:, and m:.");
                }

                // Assign each token.
                Min = minTmp;
                Value = valueTmp;
                Max = maxTmp;
                DataType = NithDataTypes.Range;

                // Compute the normalized value as ((value - min) / (max - min)) * 100
                double minDouble = double.Parse(Min, CultureInfo.InvariantCulture);
                double valueDouble = double.Parse(Value, CultureInfo.InvariantCulture);
                double maxDouble = double.Parse(Max, CultureInfo.InvariantCulture);

                if (Math.Abs(maxDouble - minDouble) < 1e-10)
                {
                    Normalized = double.NaN;
                }
                else
                {
                    Normalized = (valueDouble - minDouble) * 100 / (maxDouble - minDouble);
                }
            }
            else
            {
                // Invalid format.
                //throw new Exception("Parameter value format is invalid. Expected a single value or a composite range (b:/v:/m:) with three tokens.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NithParameterValue"/> struct using provided tokens.
        /// </summary>
        /// <param name="parameter">The parameter associated with the value.</param>
        /// <param name="min">The minimum value from the composite range.</param>
        /// <param name="value">The actual parameter value.</param>
        /// <param name="max">The maximum value from the composite range.</param>
        public NithParameterValue(NithParameters parameter, string min, string value, string max)
        {
            Parameter = parameter;
            Min = min;
            Value = value;
            Max = max;
            DataType = NithDataTypes.Range;
            double minDouble = double.Parse(Min, CultureInfo.InvariantCulture);
            double valueDouble = double.Parse(Value, CultureInfo.InvariantCulture);
            double maxDouble = double.Parse(Max, CultureInfo.InvariantCulture);
            if (Math.Abs(maxDouble - minDouble) < 1e-10)
            {
                Normalized = double.NaN;
            }
            else
            {
                Normalized = (valueDouble - minDouble) * 100 / (maxDouble - minDouble);
            }
        }

        /// <summary>
        /// Gets or sets the parameter that this value is associated with.
        /// </summary>
        public NithParameters Parameter { get; set; }

        /// <summary>
        /// In the composite range case, represents the minimum value. In the simple case, it remains empty.
        /// </summary>
        public string Min { get; set; }

        /// <summary>
        /// In both cases, represents the actual value.
        /// In the simple case, this is the only value provided.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the value as a double.
        /// </summary>
        public double ValueAsDouble
        {
            get
            {
                try
                {
                    return double.Parse(Value, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets or sets the max value as a string.
        /// In the composite case, this is the provided maximum value.
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
        /// Gets the min value as a double.
        /// </summary>
        public double MinAsDouble
        {
            get
            {
                try
                {
                    return double.Parse(Min, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets or sets the normalized value (from 0 to 100). 
        /// In the composite range case it is computed as ((value - min) / (max - min)) * 100.
        /// For a simple value (OnlyValue) Normalized is set to NaN.
        /// </summary>
        public double Normalized { get; set; }

        /// <summary>
        /// Gets or sets the type of data (OnlyValue or Range).
        /// </summary>
        public NithDataTypes DataType { get; set; }
    }
}