using NITHlibrary.Nith.Internals;
using System.Collections.Generic;
using System.Linq;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// A preprocessor that selectively filters NITH parameters based on their source sensor.
    /// This allows multiplexing data from multiple sensors by accepting specific parameters only from designated sources.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var selector = new NithPreprocessor_ParameterSelector();
    /// selector.AddRule("NITHphone", NithParameters.head_acc_yaw);
    /// selector.AddRule("NITHphone", NithParameters.head_pos_pitch);
    /// selector.AddRule("NITHbreath", NithParameters.mouth_ape);
    /// nithModule.Preprocessors.Add(selector);
    /// </code>
    /// </remarks>
    public class NithPreprocessor_ParameterSelector : INithPreprocessor
    {
        /// <summary>
        /// Defines the filtering mode for parameter selection.
        /// </summary>
        public enum FilterMode
        {
            /// <summary>
            /// Whitelist mode: Only parameters with explicit rules are accepted. All others are removed.
            /// </summary>
            Whitelist,

            /// <summary>
            /// Blacklist mode: All parameters are accepted except those with explicit deny rules.
            /// </summary>
            Blacklist
        }

        private readonly Dictionary<string, HashSet<NithParameters>> _sensorParameterRules = new();
        private FilterMode _mode = FilterMode.Whitelist;

        /// <summary>
        /// Gets or sets the filtering mode.
        /// Default is Whitelist (only explicitly allowed parameters pass through).
        /// </summary>
        public FilterMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        /// <summary>
        /// Adds a rule to accept (in Whitelist mode) or reject (in Blacklist mode) a specific parameter from a specific sensor.
        /// </summary>
        /// <param name="sensorName">The sensor name (without version, e.g., "NITHphone", "NITHbreath")</param>
        /// <param name="parameter">The parameter to filter</param>
        /// <remarks>
        /// In Whitelist mode: Only parameters added via AddRule will be kept.
        /// In Blacklist mode: Parameters added via AddRule will be removed.
        /// </remarks>
        public void AddRule(string sensorName, NithParameters parameter)
        {
            if (!_sensorParameterRules.ContainsKey(sensorName))
            {
                _sensorParameterRules[sensorName] = new HashSet<NithParameters>();
            }

            _sensorParameterRules[sensorName].Add(parameter);
        }

        /// <summary>
        /// Adds multiple rules for a specific sensor.
        /// </summary>
        /// <param name="sensorName">The sensor name (without version)</param>
        /// <param name="parameters">List of parameters to filter</param>
        public void AddRules(string sensorName, params NithParameters[] parameters)
        {
            foreach (var param in parameters)
            {
                AddRule(sensorName, param);
            }
        }

        /// <summary>
        /// Adds a rule to accept/reject ALL parameters from a specific sensor.
        /// </summary>
        /// <param name="sensorName">The sensor name (without version)</param>
        /// <remarks>
        /// This is implemented by adding a special wildcard marker.
        /// In Whitelist mode: All parameters from this sensor will pass through.
        /// In Blacklist mode: All parameters from this sensor will be blocked.
        /// </remarks>
        public void AddSensorWildcard(string sensorName)
        {
            if (!_sensorParameterRules.ContainsKey(sensorName))
            {
                _sensorParameterRules[sensorName] = new HashSet<NithParameters>();
            }

            // Use a special marker to indicate "all parameters"
            // We'll use NithParameters enum value 0 (if it exists) or just check for empty set with special flag
            // For simplicity, we'll just mark it with a comment and check in the filter logic
            _sensorParameterRules[sensorName].Clear(); // Empty set = wildcard when explicitly added
        }

        /// <summary>
        /// Removes all rules for a specific sensor.
        /// </summary>
        /// <param name="sensorName">The sensor name to clear rules for</param>
        public void ClearRulesForSensor(string sensorName)
        {
            _sensorParameterRules.Remove(sensorName);
        }

        /// <summary>
        /// Removes all configured rules.
        /// </summary>
        public void ClearAllRules()
        {
            _sensorParameterRules.Clear();
        }

        /// <summary>
        /// Gets the number of sensors with configured rules.
        /// </summary>
        public int RuleCount => _sensorParameterRules.Count;

        /// <summary>
        /// Transforms the sensor data by filtering parameters based on configured rules.
        /// </summary>
        /// <param name="sensorData">The incoming sensor data</param>
        /// <returns>The filtered sensor data with only allowed parameters</returns>
        public NithSensorData TransformData(NithSensorData sensorData)
        {
            // If no rules are configured, pass through everything unchanged
            if (_sensorParameterRules.Count == 0)
            {
                return sensorData;
            }

            // Get the sensor name from the data
            string sensorName = sensorData.SensorName;

            // Check if we have rules for this sensor
            bool hasRulesForSensor = _sensorParameterRules.ContainsKey(sensorName);

            // Create a new list for filtered values
            var filteredValues = new List<NithParameterValue>();

            foreach (var paramValue in sensorData.Values)
            {
                bool shouldKeep = ShouldKeepParameter(sensorName, paramValue.Parameter, hasRulesForSensor);

                if (shouldKeep)
                {
                    filteredValues.Add(paramValue);
                }
            }

            // Update the sensor data with filtered values
            sensorData.Values = filteredValues;

            return sensorData;
        }

        /// <summary>
        /// Determines if a parameter should be kept based on the current mode and rules.
        /// </summary>
        private bool ShouldKeepParameter(string sensorName, NithParameters parameter, bool hasRulesForSensor)
        {
            if (_mode == FilterMode.Whitelist)
            {
                // Whitelist: Keep only if explicitly allowed
                if (!hasRulesForSensor)
                {
                    // No rules for this sensor = reject all
                    return false;
                }

                var allowedParams = _sensorParameterRules[sensorName];

                // Check for wildcard (empty set means accept all from this sensor)
                if (allowedParams.Count == 0)
                {
                    return true;
                }

                // Check if this specific parameter is allowed
                return allowedParams.Contains(parameter);
            }
            else // Blacklist mode
            {
                // Blacklist: Keep everything except explicitly denied
                if (!hasRulesForSensor)
                {
                    // No rules for this sensor = accept all
                    return true;
                }

                var deniedParams = _sensorParameterRules[sensorName];

                // Check for wildcard (empty set means reject all from this sensor)
                if (deniedParams.Count == 0)
                {
                    return false;
                }

                // Check if this specific parameter is denied
                return !deniedParams.Contains(parameter);
            }
        }

        /// <summary>
        /// Gets a summary of configured rules for debugging/logging purposes.
        /// </summary>
        /// <returns>A string describing all configured rules</returns>
        public string GetRulesSummary()
        {
            if (_sensorParameterRules.Count == 0)
            {
                return $"NithPreprocessor_ParameterSelector: No rules configured (all parameters pass through)";
            }

            var summary = $"NithPreprocessor_ParameterSelector (Mode: {_mode})\n";

            foreach (var kvp in _sensorParameterRules)
            {
                string sensorName = kvp.Key;
                var parameters = kvp.Value;

                if (parameters.Count == 0)
                {
                    summary += $"  {sensorName}: ALL parameters {(_mode == FilterMode.Whitelist ? "accepted" : "rejected")}\n";
                }
                else
                {
                    string action = _mode == FilterMode.Whitelist ? "Accept" : "Reject";
                    summary += $"  {sensorName}: {action} {parameters.Count} parameter(s): ";
                    summary += string.Join(", ", parameters.Select(p => p.ToString()));
                    summary += "\n";
                }
            }

            return summary;
        }
    }
}
