using System.Globalization;
using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Mappers;

namespace NITHlibrary.Nith.Wrappers.NithFaceCam
{
    /// <summary>
    /// A wrapper for NithFaceCam software sensor.
    ///
    /// It is able to handle NithData, and turn the values into more convenient and manageable stuff, including:
    /// - Blinking (boolean) for both eyes independently and together
    /// - Mouth opened/closed (boolean)
    /// - Eye aperture (continuous value), with a maximum, discarding the minimum
    ///
    /// Moreover, it can be used to calibrate eyes and mouth aperture for the specific user, taking into account their maximum aperture and minimum aperture.
    /// </summary>
    public class NithPreprocessor_FaceCam : INithPreprocessor
    {
        // Double threshold for eye blinks?
        public bool DoubleThreshBlinks { get; set; }

        // Deadzone percentages
        private const float DEADZONE_PERC_LE = 0.2f;
        private const float DEADZONE_PERC_MOU = 0.3f;
        private const float DEADZONE_PERC_RE = 0.2f;

        // Define the percentage of opening required to consider something open (eyes and mouth)
        private const float OPENINGPERCENTAGE_LE = 0.25f;
        private const float OPENINGPERCENTAGE_MOU = 0.10f;
        private const float OPENINGPERCENTAGE_RE = 0.25f;

        // Domain mapper to turn values in the 0/100 range
        private readonly SegmentMapper mapperLE;
        private readonly SegmentMapper mapperMOU;
        private readonly SegmentMapper mapperRE;

        // Required arguments for this preprocessor to intervene
        private readonly List<NithParameters> requiredArguments = new List<NithParameters>()
        {
            NithParameters.eyeLeft_ape,
            NithParameters.eyeRight_ape,
            NithParameters.mouth_ape
        };

        // Required sensor name ofr this preprocessor to intervene

        private readonly List<string> requiredSensorName = new List<string>()
        {
            "NITHfaceCam"
        };

        private float aperture_LE = 0;
        private float aperture_MOU = 0;
        private float aperture_RE = 0;

        // Deadzones for max/min (both upper and lower)
        private float deadzone_LE = 0;
        private float deadzone_MOU = 0;
        private float deadzone_RE = 0;

        private bool isOpen_LE = false;
        private bool isOpen_MOU = false;
        private bool isOpen_RE = false;

        private float MAX_val_LE = float.NegativeInfinity;
        private float MAX_val_MOU = float.NegativeInfinity;
        private float MAX_val_RE = float.NegativeInfinity;
        private float MIN_val_LE = float.PositiveInfinity;
        private float MIN_val_MOU = float.PositiveInfinity;
        private float MIN_val_RE = float.PositiveInfinity;

        private float threshold_LE = 0;
        private float threshold_MOU = 0;
        private float threshold_RE = 0;

        private bool isCalibratingOpen = false;
        private bool isCalibratingClosed = false;

        public void Calibrate_Open()
        {
            isCalibratingOpen = true;
        }
        public void Calibrate_Closed()
        {
            isCalibratingClosed = true;
        }

        public NithPreprocessor_FaceCam(NithFaceCamCalibrationModes calibrationMode = NithFaceCamCalibrationModes.Automatic_continuous, bool doubleThreshBlinks = true)
        {
            CalibrationMode = calibrationMode;
            mapperLE = new SegmentMapper(0, 100, 0, 100, true);
            mapperRE = new SegmentMapper(0, 100, 0, 100, true);
            mapperMOU = new SegmentMapper(0, 100, 0, 100, true);
            DoubleThreshBlinks = doubleThreshBlinks;
        }

        public NithFaceCamCalibrationModes CalibrationMode { get; set; }

        NithSensorData INithPreprocessor.TransformData(NithSensorData sensorData)
        {
            if (sensorData.ContainsParameters(requiredArguments) && requiredSensorName.Contains(sensorData.SensorName)) // Check for arguments presence and correct sensor name
            {

                // Retrieve arguments as double
                float val_LE = (float)sensorData.GetParameter(NithParameters.eyeLeft_ape).Value.Base_AsDouble;
                float val_RE = (float)sensorData.GetParameter(NithParameters.eyeRight_ape).Value.Base_AsDouble;
                float val_MOU = (float)sensorData.GetParameter(NithParameters.mouth_ape).Value.Base_AsDouble; // TODO questa è a zero. Embé?

                // Calibration
                switch (CalibrationMode) // If calibration continuous, constantly update the thresholds
                {
                    case NithFaceCamCalibrationModes.Automatic_continuous:
                        // Update min and max
                        if (val_LE < MIN_val_LE) MIN_val_LE = val_LE;
                        if (val_LE > MAX_val_LE) MAX_val_LE = val_LE;
                        if (val_RE < MIN_val_RE) MIN_val_RE = val_RE;
                        if (val_RE > MAX_val_RE) MAX_val_RE = val_RE;
                        if (val_MOU < MIN_val_MOU) MIN_val_MOU = val_MOU;
                        if (val_MOU > MAX_val_MOU) MAX_val_MOU = val_MOU;
                        break;
                    case NithFaceCamCalibrationModes.Manual:
                        // Resolve calibrations
                        if (isCalibratingOpen)
                        {
                            MAX_val_LE = val_LE;
                            MAX_val_RE = val_RE;
                            isCalibratingOpen = false;
                        }
                        if (isCalibratingClosed)
                        {
                            MIN_val_LE = val_LE;
                            MIN_val_RE = val_RE;
                            isCalibratingClosed = false;
                        }
                        break;
                }
                // Update deadzones
                deadzone_LE = Math.Abs(MAX_val_LE - MIN_val_LE) * DEADZONE_PERC_LE;
                deadzone_RE = Math.Abs(MAX_val_RE - MIN_val_RE) * DEADZONE_PERC_RE;
                deadzone_MOU = Math.Abs(MAX_val_MOU - MIN_val_MOU) * DEADZONE_PERC_MOU;

                // Update thresholds (= min + OPENINGPERCENTAGE * distance between min and max)
                threshold_LE = MIN_val_LE + OPENINGPERCENTAGE_LE * Math.Abs(MAX_val_LE - MIN_val_LE);
                threshold_RE = MIN_val_RE + OPENINGPERCENTAGE_RE * Math.Abs(MAX_val_RE - MIN_val_RE);
                threshold_MOU = MIN_val_MOU + OPENINGPERCENTAGE_MOU * Math.Abs(MAX_val_MOU - MIN_val_MOU);


                // Reset mapping ranges (but introduce deadzones)

                if (MIN_val_LE + deadzone_LE < MAX_val_LE - deadzone_LE)
                    mapperLE.SetBaseRange(MIN_val_LE + deadzone_LE, MAX_val_LE - deadzone_LE);

                if (MIN_val_RE + deadzone_RE < MAX_val_RE - deadzone_RE)
                    mapperRE.SetBaseRange(MIN_val_RE + deadzone_RE, MAX_val_RE - deadzone_RE);

                if (MIN_val_MOU < MAX_val_MOU - deadzone_MOU)
                    mapperMOU.SetBaseRange(MIN_val_MOU, MAX_val_MOU - deadzone_MOU);

                // Calculate aperture percentages
                aperture_LE = (float)mapperLE.Map(val_LE);
                aperture_RE = (float)mapperRE.Map(val_RE);
                aperture_MOU = (float)mapperMOU.Map(val_MOU);

                // Application of a double threshold for blinks (one third up, one third down)
                //float threshold_LE_up = threshold_LE + (Math.Abs(MAX_val_LE - MIN_val_LE) * 0.16f);
                //float threshold_LE_down = threshold_LE - (Math.Abs(MAX_val_LE - MIN_val_LE) * 0.16f);
                //float threshold_RE_up = threshold_RE + (Math.Abs(MAX_val_RE - MIN_val_RE) * 0.16f);
                //float threshold_RE_down = threshold_RE - (Math.Abs(MAX_val_RE - MIN_val_RE) * 0.16f);

                // Wait a moment...
                float edge_up = 66f;
                float edge_down = 34f;

                isOpen_MOU = val_MOU > threshold_MOU;

                if (!DoubleThreshBlinks)
                {
                    // Calculate if eyes and mouth are open
                    isOpen_LE = val_LE > threshold_LE;
                    isOpen_RE = val_RE > threshold_RE;

                }
                else
                {
                    // Check for double threshold application
                    if (isOpen_LE)
                    {
                        if (aperture_LE < edge_down)
                        {
                            isOpen_LE = false;
                        }
                    }
                    else
                    {
                        if (aperture_LE > edge_up)
                        {
                            isOpen_LE = true;
                        }
                    }

                    if (isOpen_RE)
                    {
                        if (aperture_RE < edge_down)
                        {
                            isOpen_RE = false;
                        }
                    }
                    else
                    {
                        if (aperture_RE > edge_up)
                        {
                            isOpen_RE = true;
                        }
                    }
                }
                // Add Nith data and modify existing =====

                // Modify aperture values to include ranges =====
                // Remove old args
                sensorData.Values.RemoveAll(x => x.Argument == NithParameters.eyeLeft_ape);
                sensorData.Values.RemoveAll(x => x.Argument == NithParameters.eyeRight_ape);
                sensorData.Values.RemoveAll(x => x.Argument == NithParameters.mouth_ape);

                // Insert new args
                // Insert Open/Close eyes and mouth
                sensorData.Values.Add(new NithArgumentValue
                {
                    Argument = NithParameters.eyeLeft_isOpen,
                    Base = isOpen_LE.ToString(),
                    Type = NithDataTypes.OnlyBase
                });
                sensorData.Values.Add(new NithArgumentValue
                {
                    Argument = NithParameters.eyeRight_isOpen,
                    Base = isOpen_RE.ToString(),
                    Type = NithDataTypes.OnlyBase
                });
                sensorData.Values.Add(new NithArgumentValue
                {
                    Argument = NithParameters.mouth_isOpen,
                    Base = isOpen_MOU.ToString(),
                    Type = NithDataTypes.OnlyBase
                });

                sensorData.Values.Add(new NithArgumentValue()
                {
                    Argument = NithParameters.eyeLeft_ape,
                    Base = aperture_LE.ToString("0.00", CultureInfo.InvariantCulture),
                    Max = "100",
                    Type = NithDataTypes.BaseAndMax
                });
                sensorData.Values.Add(new NithArgumentValue()
                {
                    Argument = NithParameters.eyeRight_ape,
                    Base = aperture_RE.ToString("0.00", CultureInfo.InvariantCulture),
                    Max = "100",
                    Type = NithDataTypes.BaseAndMax
                });
                sensorData.Values.Add(new NithArgumentValue()
                {
                    Argument = NithParameters.mouth_ape,
                    Base = aperture_MOU.ToString("0.00", CultureInfo.InvariantCulture),
                    Max = "100",
                    Type = NithDataTypes.BaseAndMax
                });
            }

            // Return ==========
            return sensorData;
        }
    }
}

