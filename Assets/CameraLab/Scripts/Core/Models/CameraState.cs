using System;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class CameraState
    {
        public float apertureF = 5.6f;
        public float shutterSeconds = 1f / 125f;
        public int iso = 200;
        public float focalLengthMm = 50f;
        public float focusDistanceMeters = 2f;
        public WhiteBalancePreset whiteBalancePreset = WhiteBalancePreset.Auto;
        public bool tripodEnabled;

        public CameraState Clone()
        {
            return new CameraState
            {
                apertureF = apertureF,
                shutterSeconds = shutterSeconds,
                iso = iso,
                focalLengthMm = focalLengthMm,
                focusDistanceMeters = focusDistanceMeters,
                whiteBalancePreset = whiteBalancePreset,
                tripodEnabled = tripodEnabled
            };
        }

        public static CameraState FromDefaults(SceneCameraDefaults defaults)
        {
            return new CameraState
            {
                apertureF = defaults.apertureF,
                shutterSeconds = defaults.shutterSeconds,
                iso = defaults.iso,
                focalLengthMm = defaults.focalLengthMm,
                focusDistanceMeters = defaults.focusDistanceMeters,
                whiteBalancePreset = defaults.whiteBalancePreset,
                tripodEnabled = defaults.tripodEnabled
            };
        }
    }
}
