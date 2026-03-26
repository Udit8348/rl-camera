using System;
using System.Collections.Generic;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class SimulationResult
    {
        public float exposureOffsetStops;
        public float exposureNormalized01;
        public float dofBlurStrength;
        public float focusErrorMeters;
        public float focusSharpness01;
        public float subjectMotionBlur01;
        public float cameraShakeBlur01;
        public float totalBlur01;
        public float noiseAmount01;
        public float whiteBalanceMismatch01;
        public float finalImageQuality01;
        public List<string> debugNotes = new List<string>();

        public SimulationResult Clone()
        {
            return new SimulationResult
            {
                exposureOffsetStops = exposureOffsetStops,
                exposureNormalized01 = exposureNormalized01,
                dofBlurStrength = dofBlurStrength,
                focusErrorMeters = focusErrorMeters,
                focusSharpness01 = focusSharpness01,
                subjectMotionBlur01 = subjectMotionBlur01,
                cameraShakeBlur01 = cameraShakeBlur01,
                totalBlur01 = totalBlur01,
                noiseAmount01 = noiseAmount01,
                whiteBalanceMismatch01 = whiteBalanceMismatch01,
                finalImageQuality01 = finalImageQuality01,
                debugNotes = new List<string>(debugNotes)
            };
        }
    }
}
