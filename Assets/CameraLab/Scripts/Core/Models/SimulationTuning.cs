using System;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class SimulationTuning
    {
        public float idealExposureWindowStops = 0.5f;
        public float maxExposurePenaltyStops = 4f;
        public float focusToleranceCloseMeters = 0.03f;
        public float focusToleranceFarMeters = 0.35f;
        public float focusDistanceInfluence = 0.07f;
        public float focalLengthFocusPenalty = 0.65f;
        public float dofSeparationDistanceMax = 15f;
        public float motionBlurScalar = 18f;
        public float shakeCurveScalar = 1.25f;
        public float safeHandheldScalar = 1f;
        public float maxWhiteBalanceMismatchKelvin = 3500f;
        public float isoNoiseStopsForMax = 6f;
        public float underExposureNoiseStopsForMax = 3f;
    }
}
