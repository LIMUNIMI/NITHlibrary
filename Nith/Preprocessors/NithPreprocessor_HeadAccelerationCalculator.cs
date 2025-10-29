using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Filters.ValueFilters;
using NITHlibrary.Tools.Mappers;
using System.Globalization;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// A preprocessor that calculates head acceleration data from velocity data.
    /// If head velocity data is present but acceleration data is not,
    /// this preprocessor calculates it and adds it to the sensor data.
    /// </summary>
    public class NithPreprocessor_HeadAccelerationCalculator : INithPreprocessor
    {
        // Acceleration calculators with timestamp
        private readonly VelocityCalculatorTimestamped _yawAccelerationCalculator;
        private readonly VelocityCalculatorTimestamped _pitchAccelerationCalculator;
        private readonly VelocityCalculatorTimestamped _rollAccelerationCalculator;

        // Filters to stabilize acceleration values
        private readonly DoubleFilterMAexpDecaying _yawAccFilter;
        private readonly DoubleFilterMAexpDecaying _pitchAccFilter;
        private readonly DoubleFilterMAexpDecaying _rollAccFilter;

        // Last velocities detected
        private double _lastYawVel = 0;
        private double _lastPitchVel = 0;
        private double _lastRollVel = 0;

        // Constants for acceleration calculation
        private readonly float _accelerationSensitivity;
        private const string AccelerationNumberFormat = "0.00000";

        /// <summary>
        /// Initializes a new instance of the <see cref="NithPreprocessor_HeadAccelerationCalculator"/> class.
        /// </summary>
        /// <param name="filterAlpha">The alpha factor for the exponential moving average filter.
        /// Higher values make the response faster but less stable. Default: 0.3.</param>
        /// <param name="accelerationSensitivity">The sensitivity factor for accelerations. Default: 1.0.</param>
        /// <param name="maxSamples">The maximum number of samples to keep in history for acceleration calculation. Default: 3.</param>
        /// <param name="maxSampleAgeMs">The maximum time in milliseconds beyond which samples are considered obsolete. Default: 500.</param>
        public NithPreprocessor_HeadAccelerationCalculator(
            float filterAlpha = 0.3f,
            float accelerationSensitivity = 1.0f,
            int maxSamples = 3,
            int maxSampleAgeMs = 500)
        {
            _accelerationSensitivity = accelerationSensitivity;

            // Initialize filters with the configurable alpha value
            _yawAccFilter = new DoubleFilterMAexpDecaying(filterAlpha);
            _pitchAccFilter = new DoubleFilterMAexpDecaying(filterAlpha);
            _rollAccFilter = new DoubleFilterMAexpDecaying(filterAlpha);

            // Initialize timestamp-based acceleration calculators (reusing VelocityCalculator for derivative)
            _yawAccelerationCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
            _pitchAccelerationCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
            _rollAccelerationCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
        }

        /// <summary>
        /// Applies transformations to the data. This method will typically be called by the <see cref="NithModule"/>.
        /// </summary>
        /// <param name="sensorData">Data from the NITH sensor.</param>
        /// <returns>The transformed data.</returns>
        public NithSensorData TransformData(NithSensorData sensorData)
        {
            // Calculate accelerations if head velocity data is available but acceleration data is not
            if ((sensorData.ContainsParameter(NithParameters.head_vel_yaw) ||
                sensorData.ContainsParameter(NithParameters.head_vel_pitch) ||
                sensorData.ContainsParameter(NithParameters.head_vel_roll)) &&
                (!sensorData.ContainsParameter(NithParameters.head_acc_yaw) ||
                !sensorData.ContainsParameter(NithParameters.head_acc_pitch) ||
                !sensorData.ContainsParameter(NithParameters.head_acc_roll)))
            {
                // Use the same timestamp for all calculations in this call
                DateTime currentTime = DateTime.Now;

                // Calculate accelerations for each axis if velocity data is available
                if (sensorData.ContainsParameter(NithParameters.head_vel_yaw) &&
                    !sensorData.ContainsParameter(NithParameters.head_acc_yaw))
                {
                    double yawVel = sensorData.GetParameterValue(NithParameters.head_vel_yaw).Value.ValueAsDouble;

                    // Calculate acceleration using current timestamp (derivative of velocity)
                    float yawAcceleration = _yawAccelerationCalculator.Push((float)yawVel, currentTime);

                    // Filter acceleration to reduce noise
                    _yawAccFilter.Push(yawAcceleration * _accelerationSensitivity);
                    double filteredYawAcc = _yawAccFilter.Pull();

                    // Add the calculated acceleration value
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_acc_yaw,
                        Value = filteredYawAcc.ToString(AccelerationNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Save current velocity for next calculation
                    _lastYawVel = yawVel;
                }

                if (sensorData.ContainsParameter(NithParameters.head_vel_pitch) &&
                    !sensorData.ContainsParameter(NithParameters.head_acc_pitch))
                {
                    double pitchVel = sensorData.GetParameterValue(NithParameters.head_vel_pitch).Value.ValueAsDouble;

                    // Calculate acceleration using current timestamp
                    float pitchAcceleration = _pitchAccelerationCalculator.Push((float)pitchVel, currentTime);

                    // Filter acceleration to reduce noise
                    _pitchAccFilter.Push(pitchAcceleration * _accelerationSensitivity);
                    double filteredPitchAcc = _pitchAccFilter.Pull();

                    // Add the calculated acceleration value
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_acc_pitch,
                        Value = filteredPitchAcc.ToString(AccelerationNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Save current velocity for next calculation
                    _lastPitchVel = pitchVel;
                }

                if (sensorData.ContainsParameter(NithParameters.head_vel_roll) &&
                    !sensorData.ContainsParameter(NithParameters.head_acc_roll))
                {
                    double rollVel = sensorData.GetParameterValue(NithParameters.head_vel_roll).Value.ValueAsDouble;

                    // Calculate acceleration using current timestamp
                    float rollAcceleration = _rollAccelerationCalculator.Push((float)rollVel, currentTime);

                    // Filter acceleration to reduce noise
                    _rollAccFilter.Push(rollAcceleration * _accelerationSensitivity);
                    double filteredRollAcc = _rollAccFilter.Pull();

                    // Add the calculated acceleration value
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_acc_roll,
                        Value = filteredRollAcc.ToString(AccelerationNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Save current velocity for next calculation
                    _lastRollVel = rollVel;
                }
            }

            return sensorData;
        }
    }
}
