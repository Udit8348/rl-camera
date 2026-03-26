using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CameraLab/Camera Preset", fileName = "CameraPreset")]
    public class CameraPreset : ScriptableObject
    {
        public string presetName = "Training Camera";
        public SensorFormat sensorFormat = SensorFormat.FullFrame;
        public float minAperture = 1.8f;
        public float maxAperture = 16f;
        public float minShutter = 1f / 4000f;
        public float maxShutter = 1f;
        public int minISO = 100;
        public int maxISO = 6400;
        public float minFocalLength = 24f;
        public float maxFocalLength = 120f;
    }
}
