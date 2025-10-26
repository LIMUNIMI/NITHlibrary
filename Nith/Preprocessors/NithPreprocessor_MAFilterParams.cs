using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Filters.ValueFilters;
using System.Globalization;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// A preprocessor which applies a moving average exponential decaying filter 
    /// to each of the specified NITH parameters.
    /// </summary>
    public class NithPreprocessor_MAfilterParams : INithPreprocessor
    {
        private readonly List<DoubleFilterMAexpDecaying> _filtersArray;
        private readonly List<NithParameters> _paramsArray;
        private const string NumberFormat = "0.00000";

        /// <summary>
        /// Initializes a new instance of the <see cref="NithPreprocessor_MAfilterParams"/> class.
        /// </summary>
        /// <param name="paramsArray">List of parameters to be filtered.</param>
        /// <param name="filterAlpha">Alpha value for the exponential decaying filter.</param>
        public NithPreprocessor_MAfilterParams(List<NithParameters> paramsArray, float filterAlpha)
        {
            this._paramsArray = paramsArray;
            _filtersArray = [];

            foreach (var argument in paramsArray)
            {
                _filtersArray.Add(new(filterAlpha));
            }
        }

        /// <summary>
        /// Transforms the given sensor data by applying the filter to each specified parameter.
        /// This method will typically be called automatically by the <see cref="NithModule"/> class."/>
        /// </summary>
        /// <param name="sensorData">The sensor data to be transformed.</param>
        /// <returns>The transformed sensor data with filtered parameters.</returns>
        public NithSensorData TransformData(NithSensorData sensorData)
        {
            for (var i = 0; i < _paramsArray.Count; i++)
            {
                var param = _paramsArray[i];
                if (sensorData.ContainsParameter(param))
                {
                    // Getting value and removing from data
                    var backupValue = sensorData.GetParameterValue(param);
                    if (!backupValue.HasValue)
                        continue;

                    var backup = backupValue.Value;
                    sensorData.Values.Remove(sensorData.Values.Find(x => x.Parameter == param));
                    var baseVal = backup.ValueAsDouble;

                    // Filtering
                    _filtersArray[i].Push(baseVal);
                    var filteredBaseVal = _filtersArray[i].Pull();

                    // Making the new value - preserve Min/Max for Range type parameters
                    if (backup.DataType == NithDataTypes.Range)
                    {
                        // For Range type, create with Min, Value, Max
                        sensorData.Values.Add(new()
                        {
                            DataType = NithDataTypes.Range,
                            Parameter = param,
                            Min = backup.Min,
                            Value = filteredBaseVal.ToString(NumberFormat, CultureInfo.InvariantCulture),
                            Max = backup.Max
                        });
                    }
                    else
                    {
                        // For OnlyValue type, create simple value
                        sensorData.Values.Add(new()
                        {
                            DataType = NithDataTypes.OnlyValue,
                            Parameter = param,
                            Value = filteredBaseVal.ToString(NumberFormat, CultureInfo.InvariantCulture),
                            Min = "",
                            Max = ""
                        });
                    }
                }
            }

            return sensorData;
        }
    }
}