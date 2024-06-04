using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Filters.ValueFilters;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// A preprocessor which applies a specific filter to each of the specified params
    /// </summary>
    public class NithPreprocessor_MAFilterParams : INithPreprocessor
    {
        private List<DoubleFilterMAExpDecaying> filtersArray;
        private List<NithParameters> paramsArray;

        public NithPreprocessor_MAFilterParams(List<NithParameters> paramsArray, float filterAlpha)
        {
            this.paramsArray = paramsArray;
            filtersArray = new List<DoubleFilterMAExpDecaying>();

            foreach (var argument in paramsArray)
            {
                filtersArray.Add(new DoubleFilterMAExpDecaying(filterAlpha));
            }
        }

        public NithSensorData TransformData(NithSensorData sensorData)
        {
            for (int i = 0; i < paramsArray.Count; i++)
            {
                NithParameters param = paramsArray[i];
                if (sensorData.ContainsParameter(param))
                {
                    // Getting value and removing from data
                    NithArgumentValue backup = sensorData.GetParameter(param).Value;
                    sensorData.Values.Remove(sensorData.Values.Find(x => x.Argument == param));
                    double baseVal = backup.Base_AsDouble;

                    // Filtering
                    filtersArray[i].Push(baseVal);
                    double filteredBaseVal = filtersArray[i].Pull();

                    // Making the new value
                    sensorData.Values.Add(new NithArgumentValue
                    {
                        Type = backup.Type,
                        Argument = param,
                        Base = filteredBaseVal.ToString("F5"),
                        Max = backup.Max
                    });
                }
            }

            return sensorData;
        }
    }
}