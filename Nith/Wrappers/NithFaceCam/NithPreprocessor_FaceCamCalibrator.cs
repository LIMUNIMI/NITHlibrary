using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Nith.Preprocessors;
using NITHlibrary.Tools.Mappers;
using System.Globalization;

namespace NITHlibrary.Nith.Wrappers.NithFaceCam
{
    /// <summary>
    /// Preprocessor for NITHwebcamWrapper.
    /// Does the following:
    /// - Eyes and mouth aperture calibration (taking into account the user's maximum and minimum aperture). Eyes and mouth aperture values will then be from 0 to 1 (and the Normalized value will become available)
    /// - Extracting the boolean values for mouth and eyes aperture (which simply state if the aperture is above a certain threshold)
    /// </summary>
    public class NithPreprocessor_FaceCam : INithPreprocessor
    {
        // Deadzone percentages
        private const float DeadzonePercLe = 0.2f;

        private const float DeadzonePercMou = 0.4f;

        private const float DeadzonePercRe = 0.2f;

        // Define the percentage of opening required to consider something open (eyes and mouth)
        private const float OpeningpercentageLe = 0.18f;

        private const float OpeningpercentageMou = 0.25f;

        private const float OpeningpercentageRe = 0.18f;

        // Domain mapper to turn values in the 0/100 range
        private readonly SegmentMapper _mapperLe;

        private readonly SegmentMapper _mapperMou;

        private readonly SegmentMapper _mapperRe;

        // Required arguments for this preprocessor to intervene
        private readonly List<NithParameters> _requiredArguments =
        [
            NithParameters.eyeLeft_ape,
            NithParameters.eyeRight_ape,
            NithParameters.mouth_ape
        ];

        private readonly List<string> _requiredSensorName = ["NITHfaceCam"];

        // Required sensor name ofr this preprocessor to intervene
        private float _apertureLe = 0;

        private float _apertureMou = 0;

        private float _apertureRe = 0;

        // Deadzones for max/min (both upper and lower)
        private float _deadzoneLe = 0;

        private float _deadzoneMou = 0;

        private float _deadzoneRe = 0;

        private bool _isCalibratingClosed = false;

        private bool _isCalibratingOpen = false;

        private bool _isOpenLe = false;

        private bool _isOpenMou = false;

        private bool _isOpenRe = false;

        private float _maxValLe = float.NegativeInfinity;

        private float _maxValMou = float.NegativeInfinity;

        private float _maxValRe = float.NegativeInfinity;

        private float _minValLe = float.PositiveInfinity;

        private float _minValMou = float.PositiveInfinity;

        private float _minValRe = float.PositiveInfinity;

        private float _thresholdLe = 0;

        private float _thresholdMou = 0;

        private float _thresholdRe = 0;

        /// <summary>
        /// Creates a new instance of the <see cref="NithPreprocessor_FaceCam"/> class.
        /// </summary>
        /// <param name="calibrationMode">Specifies how the calibration will be performed.</param>
        /// <param name="doubleThreshBlinks">Should a double threshold be applied for blink detection? (Usually more robust)</param>
        public NithPreprocessor_FaceCam(NithFaceCamCalibrationModes calibrationMode = NithFaceCamCalibrationModes.AutomaticContinuous, bool doubleThreshBlinks = true)
        {
            CalibrationMode = calibrationMode;
            _mapperLe = new(0, 100, 0, 100, true);
            _mapperRe = new(0, 100, 0, 100, true);
            _mapperMou = new(0, 100, 0, 100, true);
            DoubleThreshBlinks = doubleThreshBlinks;
        }

        /// <summary>
        /// Specifies the calibration mode.
        /// </summary>
        public NithFaceCamCalibrationModes CalibrationMode { get; set; }

        /// <summary>
        /// Specifies if a double threshold should be applied for the detecton of eye blinks (usually more robust).
        /// </summary>
        public bool DoubleThreshBlinks { get; set; }

        /// <summary>
        /// If <see cref="NithFaceCamCalibrationModes.Manual"/> is set, this method should be called to calibrate the closed state. To do this, the user should close both eyes and mouth.
        /// </summary>
        public void Calibrate_Closed()
        {
            _isCalibratingClosed = true;
        }

        /// <summary>
        /// If <see cref="NithFaceCamCalibrationModes.Manual"/> is set, this method should be called to calibrate the open state. To do this, the user should open both eyes and mouth.
        /// </summary>
        public void Calibrate_Open()
        {
            _isCalibratingOpen = true;
        }

        /// <summary>
        /// Applies the transformations to the data. This method will be typically called by the associated <see cref="NithModule"/>.
        /// </summary>
        /// <param name="sensorData">Incoming sensor data.</param>
        /// <returns>Transformed sensor data.</returns>
        NithSensorData INithPreprocessor.TransformData(NithSensorData sensorData)
        {
            if (sensorData.ContainsParameters(_requiredArguments) && _requiredSensorName.Contains(sensorData.SensorName)) // Check for arguments presence and correct sensor name
            {
                // Retrieve arguments as double
                var valLe = (float)sensorData.GetParameterValue(NithParameters.eyeLeft_ape).Value.BaseAsDouble;
                var valRe = (float)sensorData.GetParameterValue(NithParameters.eyeRight_ape).Value.BaseAsDouble;
                var valMou = (float)sensorData.GetParameterValue(NithParameters.mouth_ape).Value.BaseAsDouble; // TODO questa è a zero. Embé?

                // Calibration
                switch (CalibrationMode) // If calibration continuous, constantly update the thresholds
                {
                    case NithFaceCamCalibrationModes.AutomaticContinuous:
                        // Update min and max
                        if (valLe < _minValLe) _minValLe = valLe;
                        if (valLe > _maxValLe) _maxValLe = valLe;
                        if (valRe < _minValRe) _minValRe = valRe;
                        if (valRe > _maxValRe) _maxValRe = valRe;
                        if (valMou < _minValMou) _minValMou = valMou;
                        if (valMou > _maxValMou) _maxValMou = valMou;
                        break;

                    case NithFaceCamCalibrationModes.Manual:
                        // Resolve calibrations
                        if (_isCalibratingOpen)
                        {
                            _maxValLe = valLe;
                            _maxValRe = valRe;
                            _maxValMou = valMou;
                            _isCalibratingOpen = false;
                        }
                        if (_isCalibratingClosed)
                        {
                            _minValLe = valLe;
                            _minValRe = valRe;
                            _minValMou = valMou;
                            _isCalibratingClosed = false;
                        }
                        break;
                }
                // Update deadzones
                _deadzoneLe = Math.Abs(_maxValLe - _minValLe) * DeadzonePercLe;
                _deadzoneRe = Math.Abs(_maxValRe - _minValRe) * DeadzonePercRe;
                _deadzoneMou = Math.Abs(_maxValMou - _minValMou) * DeadzonePercMou;

                // Update thresholds (= min + OPENINGPERCENTAGE * distance between min and max)
                _thresholdLe = _minValLe + OpeningpercentageLe * Math.Abs(_maxValLe - _minValLe);
                _thresholdRe = _minValRe + OpeningpercentageRe * Math.Abs(_maxValRe - _minValRe);
                _thresholdMou = _minValMou + OpeningpercentageMou * Math.Abs(_maxValMou - _minValMou);

                // Reset mapping ranges (but introduce deadzones)

                if (_minValLe + _deadzoneLe < _maxValLe - _deadzoneLe)
                    _mapperLe.SetBaseRange(_minValLe + _deadzoneLe, _maxValLe - _deadzoneLe);

                if (_minValRe + _deadzoneRe < _maxValRe - _deadzoneRe)
                    _mapperRe.SetBaseRange(_minValRe + _deadzoneRe, _maxValRe - _deadzoneRe);

                if (_minValMou < _maxValMou - _deadzoneMou)
                    _mapperMou.SetBaseRange(_minValMou, _maxValMou - _deadzoneMou);

                // Calculate aperture percentages
                _apertureLe = (float)_mapperLe.Map(valLe);
                _apertureRe = (float)_mapperRe.Map(valRe);
                _apertureMou = (float)_mapperMou.Map(valMou);

                // Application of a double threshold for blinks (one third up, one third down)
                //float threshold_LE_up = threshold_LE + (Math.Abs(MAX_val_LE - MIN_val_LE) * 0.16f);
                //float threshold_LE_down = threshold_LE - (Math.Abs(MAX_val_LE - MIN_val_LE) * 0.16f);
                //float threshold_RE_up = threshold_RE + (Math.Abs(MAX_val_RE - MIN_val_RE) * 0.16f);
                //float threshold_RE_down = threshold_RE - (Math.Abs(MAX_val_RE - MIN_val_RE) * 0.16f);

                // Wait a moment...
                var edgeUp = 66f;
                var edgeDown = 34f;

                _isOpenMou = valMou > _thresholdMou;

                if (!DoubleThreshBlinks)
                {
                    // Calculate if eyes and mouth are open
                    _isOpenLe = valLe > _thresholdLe;
                    _isOpenRe = valRe > _thresholdRe;
                }
                else
                {
                    // Check for double threshold application
                    if (_isOpenLe)
                    {
                        if (_apertureLe < edgeDown)
                        {
                            _isOpenLe = false;
                        }
                    }
                    else
                    {
                        if (_apertureLe > edgeUp)
                        {
                            _isOpenLe = true;
                        }
                    }

                    if (_isOpenRe)
                    {
                        if (_apertureRe < edgeDown)
                        {
                            _isOpenRe = false;
                        }
                    }
                    else
                    {
                        if (_apertureRe > edgeUp)
                        {
                            _isOpenRe = true;
                        }
                    }
                }
                // Add Nith data and modify existing =====

                // Modify aperture values to include ranges =====
                // Remove old args
                sensorData.Values.RemoveAll(x => x.Parameter == NithParameters.eyeLeft_ape);
                sensorData.Values.RemoveAll(x => x.Parameter == NithParameters.eyeRight_ape);
                sensorData.Values.RemoveAll(x => x.Parameter == NithParameters.mouth_ape);

                // Insert new args
                // Insert Open/Close eyes and mouth
                sensorData.Values.Add(new()
                {
                    Parameter = NithParameters.eyeLeft_isOpen,
                    Base = _isOpenLe.ToString(),
                    Type = NithDataTypes.OnlyBase
                });
                sensorData.Values.Add(new()
                {
                    Parameter = NithParameters.eyeRight_isOpen,
                    Base = _isOpenRe.ToString(),
                    Type = NithDataTypes.OnlyBase
                });
                sensorData.Values.Add(new()
                {
                    Parameter = NithParameters.mouth_isOpen,
                    Base = _isOpenMou.ToString(),
                    Type = NithDataTypes.OnlyBase
                });

                sensorData.Values.Add(new(NithParameters.eyeLeft_ape, _apertureLe.ToString("0.00", CultureInfo.InvariantCulture), "100"));
                sensorData.Values.Add(new(NithParameters.eyeRight_ape, _apertureRe.ToString("0.00", CultureInfo.InvariantCulture), "100"));
                sensorData.Values.Add(new(NithParameters.mouth_ape, _apertureMou.ToString("0.00", CultureInfo.InvariantCulture), "100"));
            }

            // Return ==========
            return sensorData;
        }
    }
}