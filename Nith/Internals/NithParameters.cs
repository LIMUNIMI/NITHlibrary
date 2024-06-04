namespace NITHlibrary.Nith.Internals
{
    public enum NithParameters
    {
        NaA,                         // Not an Argument

        // Head presence
        head_presence,

        // Gyroscopic head rotation (position)
        head_pos_yaw,
        head_pos_pitch,
        head_pos_roll,

        // Gyroscopic head rotation (acceleration)
        head_acc_yaw,
        head_acc_pitch,
        head_acc_roll,

        // Breath pressure
        breath_press,

        // Teeth pressure
        teeth_press,

        // Eyes aperture (continuous and boolean)
        eyeLeft_ape,
        eyeRight_ape,
        eyeLeft_isOpen,
        eyeRight_isOpen,

        // Eyes position in space
        eyeLeft_pos_x,
        eyeLeft_pos_y,
        eyeLeft_pos_z,
        eyeRight_pos_x,
        eyeRight_pos_y,
        eyeRight_pos_z,

        // Eyes presence
        eyes_presence,

        // Gaze coordinates
        gaze_x,
        gaze_y,

        // Gaze presence //TODO temp
        gaze_presence,

        // Mouth aperture (continuous and boolean)
        mouth_ape,
        mouth_isOpen,

        // Calibration data
        cal_gyro, // Gyroscope
        cal_acc, // Accelerometer
        cal_mag, // Magnetometer
        cal_sys, // General system
    }
}