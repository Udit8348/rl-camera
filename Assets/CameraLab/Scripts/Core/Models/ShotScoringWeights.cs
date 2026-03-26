using System;

namespace CameraLab.Core.Models
{
    [Serializable]
    public struct ShotScoringWeights
    {
        public float exposureWeight;
        public float sharpnessWeight;
        public float noiseWeight;
        public float whiteBalanceWeight;

        public static ShotScoringWeights Default =>
            new ShotScoringWeights
            {
                exposureWeight = 0.3f,
                sharpnessWeight = 0.4f,
                noiseWeight = 0.15f,
                whiteBalanceWeight = 0.15f
            };
    }
}
