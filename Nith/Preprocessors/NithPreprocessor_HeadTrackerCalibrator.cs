using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Mappers;
using NITHlibrary.Tools.Types;
using System.Globalization;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// A preprocessor which provides a simple utility to calibrate head tracker data. It allows to set a center position and then calibrate the incoming data to that center.
    /// </summary>
    public class NithPreprocessor_HeadTrackerCalibrator : INithPreprocessor
    {
        private const string NumberFormat = "0.00000";
        private readonly AngleBaseChanger _pitchBaseChanger = new();

        private readonly List<NithParameters> _requiredArguments =
            [NithParameters.head_pos_yaw, NithParameters.head_pos_pitch, NithParameters.head_pos_yaw];

        private readonly AngleBaseChanger _rollBaseChanger = new();
        private readonly AngleBaseChanger _yawBaseChanger = new();

        private Polar3DData CenteredPosition => new() { Yaw = _yawBaseChanger.Transform(Position.Yaw), Pitch = _pitchBaseChanger.Transform(Position.Pitch), Roll = _rollBaseChanger.Transform(Position.Roll) };

        private Polar3DData Position { get; set; }

        /// <summary>
        /// Get the actual center position, in 3D polar coordinates (yaw, pitch, roll).
        /// </summary>
        /// <returns>The actual center position</returns>
        public Polar3DData GetCenter()
        {
            return new()
            {
                Yaw = _yawBaseChanger.Delta,
                Pitch = _pitchBaseChanger.Delta,
                Roll = _rollBaseChanger.Delta
            };
        }

        /// <summary>
        /// Set the center position to a specific value, in 3D polar coordinates (yaw, pitch, roll).
        /// </summary>
        /// <param name="center">The specific center to be set.</param>
        public void SetCenter(Polar3DData center)
        {
            _pitchBaseChanger.Delta = center.Pitch;
            _yawBaseChanger.Delta = center.Yaw;
            _rollBaseChanger.Delta = center.Roll;
        }

        /// <summary>
        /// Set the center position to the current head position.
        /// </summary>
        public void SetCenterToCurrentPosition()
        {
            _pitchBaseChanger.Delta = Position.Pitch;
            _yawBaseChanger.Delta = Position.Yaw;
            _rollBaseChanger.Delta = Position.Roll;
        }

        /// <summary>
        /// Applies the transformations to the data. This method will be typically called by the <see cref="NithModule"/>.
        /// </summary>
        /// <param name="sensorData">Data coming from the NITH sensor.</param>
        /// <returns>The transformed data.</returns>
        public NithSensorData TransformData(NithSensorData sensorData)
        {
            if (sensorData.ContainsParameters(_requiredArguments))
            {
                ParsePositionFromNithValues(sensorData.Values);

                // Remove previous values
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Parameter == NithParameters.head_pos_yaw));
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Parameter == NithParameters.head_pos_pitch));
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Parameter == NithParameters.head_pos_roll));

                // Add calibrated values
                sensorData.Values.Add(new()
                {
                    Type = NithDataTypes.OnlyBase,
                    Parameter = NithParameters.head_pos_yaw,
                    Base = CenteredPosition.Yaw.ToString(NumberFormat, CultureInfo.InvariantCulture),
                });
                sensorData.Values.Add(new()
                {
                    Type = NithDataTypes.OnlyBase,
                    Parameter = NithParameters.head_pos_pitch,
                    Base = CenteredPosition.Pitch.ToString(NumberFormat, CultureInfo.InvariantCulture),
                });
                sensorData.Values.Add(new()
                {
                    Type = NithDataTypes.OnlyBase,
                    Parameter = NithParameters.head_pos_roll,
                    Base = CenteredPosition.Roll.ToString(NumberFormat, CultureInfo.InvariantCulture),
                });
            }
            return sensorData;
        }

        private void ParsePositionFromNithValues(List<NithParameterValue> values)
        {
            double posY, posP, posR;
            posY = posP = posR = 0;

            foreach (var arg in values)
            {
                switch (arg.Parameter)
                {
                    case NithParameters.head_pos_yaw: posY = arg.BaseAsDouble; break;
                    case NithParameters.head_pos_pitch: posP = arg.BaseAsDouble; break;
                    case NithParameters.head_pos_roll: posR = arg.BaseAsDouble; break;
                    default: break;
                }
            }

            Position = new() { Yaw = posY, Pitch = posP, Roll = posR };
        }
    }
}