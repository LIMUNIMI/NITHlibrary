namespace NITHlibrary.Nith.Internals
{
    /// <summary>
    /// Various parameters that can be output from a NithSensor.
    /// </summary>
    public enum NithParameters
    {
        /// <summary>
        /// Not a Parameter
        /// </summary>
        NaP,

        #region Eyes

        /// <summary>
        /// Left eye aperture (continuous)
        /// </summary>
        eyeLeft_ape,

        /// <summary>
        /// Right eye aperture (continuous)
        /// </summary>
        eyeRight_ape,

        /// <summary>
        /// Left eye open status (boolean)
        /// </summary>
        eyeLeft_isOpen,

        /// <summary>
        /// Right eye open status (boolean)
        /// </summary>
        eyeRight_isOpen,

        /// <summary>
        /// Left eye position in X space
        /// </summary>
        eyeLeft_pos_x,

        /// <summary>
        /// Left eye position in Y space
        /// </summary>
        eyeLeft_pos_y,

        /// <summary>
        /// Left eye position in Z space
        /// </summary>
        eyeLeft_pos_z,

        /// <summary>
        /// Right eye position in X space
        /// </summary>
        eyeRight_pos_x,

        /// <summary>
        /// Right eye position in Y space
        /// </summary>
        eyeRight_pos_y,

        /// <summary>
        /// Right eye position in Z space
        /// </summary>
        eyeRight_pos_z,

        /// <summary>
        /// Left eye angular displacement on X-axis
        /// </summary>
        eyeLeft_ang_x,

        /// <summary>
        /// Left eye angular displacement on Y-axis
        /// </summary>
        eyeLeft_ang_y,

        /// <summary>
        /// Right eye angular displacement on X-axis
        /// </summary>
        eyeRight_ang_x,

        /// <summary>
        /// Right eye angular displacement on Y-axis
        /// </summary>
        eyeRight_ang_y,

        /// <summary>
        /// Left eyebrow height
        /// </summary>
        eyeLeft_brow_height,

        /// <summary>
        /// Right eyebrow height
        /// </summary>
        eyeRight_brow_height,

        /// <summary>
        /// Left eyebrow phase (-1: down, 0: neutral, 1: up)
        /// </summary>
        eyeLeft_brow_phase,

        /// <summary>
        /// Right eyebrow phase (-1: down, 0: neutral, 1: up)
        /// </summary>
        eyeRight_brow_phase,

        /// <summary>
        /// Eyes presence
        /// </summary>
        eyes_presence,

        /// <summary>
        /// Gaze coordinates on X-axis
        /// </summary>
        gaze_x,

        /// <summary>
        /// Gaze coordinates on Y-axis
        /// </summary>
        gaze_y,

        /// <summary>
        /// Gaze presence
        /// </summary>
        gaze_presence,

        #endregion Eyes

        #region Mouth

        /// <summary>
        /// Voice pitch frequency
        /// </summary>
        voice_pitch,

        /// <summary>
        /// Voice intensity (volume)
        /// </summary>
        voice_intensity,

        /// <summary>
        /// Whistle pitch (frequency)
        /// </summary>
        whistle_pitch,

        /// <summary>
        /// Whistle intensity (volume)
        /// </summary>
        whistle_intensity,

        /// <summary>
        /// Breath pressure
        /// </summary>
        breath_press,

        /// <summary>
        /// Mouth aperture (continuous)
        /// </summary>
        mouth_ape,
        
        /// <summary>
        /// Mouth height
        /// </summary>
        mouth_height,

        /// <summary>
        /// Mouth width
        /// </summary>
        mouth_width,

        /// <summary>
        /// Mouth open status (boolean)
        /// </summary>
        mouth_isOpen,

        /// <summary>
        /// Teeth pressure
        /// </summary>
        teeth_press,

        /// <summary>
        /// Jaw position on X-axis
        /// </summary>
        jaw_x,

        /// <summary>
        /// Jaw position on Y-axis
        /// </summary>
        jaw_y,

        /// <summary>
        /// Jaw position on Z-axis
        /// </summary>
        jaw_z,

        /// <summary>
        /// Tongue position in the space on X-axis
        /// </summary>
        tongue_free_x,

        /// <summary>
        /// Tongue position in the space on Y-axis
        /// </summary>
        tongue_free_y,

        /// <summary>
        /// Tongue position in the space on Z-axis
        /// </summary>
        tongue_free_z,

        /// <summary>
        /// Tongue position on the palate on X-axis
        /// </summary>
        tongue_palate_x,

        /// <summary>
        /// Tongue position on the palate on Y-axis
        /// </summary>
        tongue_palate_y,

        /// <summary>
        /// Tongue pressure on the palate
        /// </summary>
        tongue_palate_pressure,

        #endregion Mouth

        #region Head

        /// <summary>
        /// Head presence
        /// </summary>
        head_presence,

        /// <summary>
        /// Gyroscopic head rotation (yaw axis position)
        /// </summary>
        head_pos_yaw,

        /// <summary>
        /// Gyroscopic head rotation (pitch axis position)
        /// </summary>
        head_pos_pitch,

        /// <summary>
        /// Gyroscopic head rotation (roll axis position)
        /// </summary>
        head_pos_roll,

        /// <summary>
        /// Gyroscopic head rotation (yaw axis acceleration)
        /// </summary>
        head_acc_yaw,

        /// <summary>
        /// Gyroscopic head rotation (pitch axis acceleration)
        /// </summary>
        head_acc_pitch,

        /// <summary>
        /// Gyroscopic head rotation (roll axis acceleration)
        /// </summary>
        head_acc_roll,

        /// <summary>
        /// Gyroscopic head rotation (yaw axis velocity)
        /// </summary>
        head_vel_yaw,

        /// <summary>
        /// Gyroscopic head rotation (pitch axis velocity)
        /// </summary>
        head_vel_pitch,

        /// <summary>
        /// Gyroscopic head rotation (roll axis velocity)
        /// </summary>
        head_vel_roll,

        /// <summary>
        /// Neck muscles tension
        /// </summary>
        neck_tension,

        #endregion Head

        #region System

        /// <summary>
        /// General calibration status
        /// </summary>
        cal_sys,

        #endregion System
    }
}