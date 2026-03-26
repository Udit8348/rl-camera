using System;

namespace CameraLab.Core.Models
{
    [Serializable]
    public struct SceneCameraDefaults
    {
        public float apertureF;
        public float shutterSeconds;
        public int iso;
        public float focalLengthMm;
        public float focusDistanceMeters;
        public WhiteBalancePreset whiteBalancePreset;
        public bool tripodEnabled;
    }
}
