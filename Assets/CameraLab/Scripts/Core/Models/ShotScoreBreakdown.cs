using System;
using System.Collections.Generic;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class ShotScoreBreakdown
    {
        public float exposureScore01;
        public float sharpnessScore01;
        public float noiseScore01;
        public float whiteBalanceScore01;
        public float overallScore01;
        public bool passFail;
        public List<string> objectiveNotes = new List<string>();

        public ShotScoreBreakdown Clone()
        {
            return new ShotScoreBreakdown
            {
                exposureScore01 = exposureScore01,
                sharpnessScore01 = sharpnessScore01,
                noiseScore01 = noiseScore01,
                whiteBalanceScore01 = whiteBalanceScore01,
                overallScore01 = overallScore01,
                passFail = passFail,
                objectiveNotes = new List<string>(objectiveNotes)
            };
        }
    }
}
