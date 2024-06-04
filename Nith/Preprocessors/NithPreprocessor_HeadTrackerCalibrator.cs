using System.Globalization;
using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Mappers;
using NITHlibrary.Tools.Types;

namespace NITHlibrary.Nith.Preprocessors
{
    public class NithPreprocessor_HeadTrackerCalibrator : INithPreprocessor
    {
        private const string NUMBER_FORMAT = "0.00000";
        private readonly AngleBaseChanger pitchBaseChanger;
        private readonly AngleBaseChanger rollBaseChanger;
        private readonly AngleBaseChanger yawBaseChanger;

        private readonly List<NithParameters> requiredArguments = new List<NithParameters>()
        {
            NithParameters.head_pos_yaw, NithParameters.head_pos_pitch, NithParameters.head_pos_yaw
        };

        public NithPreprocessor_HeadTrackerCalibrator()
        {
            pitchBaseChanger = new AngleBaseChanger();
            yawBaseChanger = new AngleBaseChanger();
            rollBaseChanger = new AngleBaseChanger();
        }

        private Polar3DData CenteredPosition
        {
            get
            {
                return new Polar3DData { Yaw = yawBaseChanger.Transform(Position.Yaw), Pitch = pitchBaseChanger.Transform(Position.Pitch), Roll = rollBaseChanger.Transform(Position.Roll) };
            }
        }

        private Polar3DData Position { get; set; }

        public Polar3DData GetCenter()
        {
            return new Polar3DData
            {
                Yaw = yawBaseChanger.Delta,
                Pitch = pitchBaseChanger.Delta,
                Roll = rollBaseChanger.Delta
            };
        }

        private void ParsePositionFromNithValues(List<NithArgumentValue> values)
        {
            double _posY, _posP, _posR;
            _posY = _posP = _posR = 0;

            foreach (NithArgumentValue arg in values)
            {
                switch (arg.Argument)
                {
                    case NithParameters.head_pos_yaw: _posY = arg.Base_AsDouble; break;
                    case NithParameters.head_pos_pitch: _posP = arg.Base_AsDouble; break;
                    case NithParameters.head_pos_roll: _posR = arg.Base_AsDouble; break;
                    default: break;
                }
            }

            Position = new Polar3DData { Yaw = _posY, Pitch = _posP, Roll = _posR };
        }

        public void SetCenterToCurrentPosition()
        {
            pitchBaseChanger.Delta = Position.Pitch;
            yawBaseChanger.Delta = Position.Yaw;
            rollBaseChanger.Delta = Position.Roll;
        }

        public void SetCenter(Polar3DData center)
        {
            pitchBaseChanger.Delta = center.Pitch;
            yawBaseChanger.Delta = center.Yaw;
            rollBaseChanger.Delta = center.Roll;
        }

        public NithSensorData TransformData(NithSensorData sensorData)
        {
            if (sensorData.ContainsParameters(requiredArguments))
            {
                ParsePositionFromNithValues(sensorData.Values);

                // Remove previous values
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Argument == NithParameters.head_pos_yaw));
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Argument == NithParameters.head_pos_pitch));
                sensorData.Values.Remove(sensorData.Values.Find(x => x.Argument == NithParameters.head_pos_roll));

                // Add calibrated values
                sensorData.Values.Add(new NithArgumentValue
                {
                    Type = NithDataTypes.OnlyBase,
                    Argument = NithParameters.head_pos_yaw,
                    Base = CenteredPosition.Yaw.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture),
                });
                sensorData.Values.Add(new NithArgumentValue
                {
                    Type = NithDataTypes.OnlyBase,
                    Argument = NithParameters.head_pos_pitch,
                    Base = CenteredPosition.Pitch.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture),
                });
                sensorData.Values.Add(new NithArgumentValue
                {
                    Type = NithDataTypes.OnlyBase,
                    Argument = NithParameters.head_pos_roll,
                    Base = CenteredPosition.Roll.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture),
                });
            }
            return sensorData;
        }
    }
}