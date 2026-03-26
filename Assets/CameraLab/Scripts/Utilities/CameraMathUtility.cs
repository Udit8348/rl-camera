using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.Utilities
{
    public static class CameraMathUtility
    {
        public static float Log2(float value)
        {
            return Mathf.Log(value, 2f);
        }

        public static float Remap01(float value, float min, float max)
        {
            if (Mathf.Approximately(min, max))
            {
                return 0f;
            }

            return Mathf.Clamp01((value - min) / (max - min));
        }

        public static float InverseRemap01(float value, float min, float max)
        {
            return 1f - Remap01(value, min, max);
        }

        public static float EvaluateRangeScore(float value, FloatRange range, float softness)
        {
            if (range.Contains(value))
            {
                return 1f;
            }

            var distance = range.DistanceFrom(value);
            return Mathf.Clamp01(1f - distance / Mathf.Max(softness, 0.0001f));
        }

        public static float GetSensorHeightMm(SensorFormat sensorFormat)
        {
            switch (sensorFormat)
            {
                case SensorFormat.APSC:
                    return 15.6f;
                case SensorFormat.MicroFourThirds:
                    return 13f;
                default:
                    return 24f;
            }
        }

        public static float CalculateVerticalFieldOfView(float focalLengthMm, SensorFormat sensorFormat)
        {
            var sensorHeight = GetSensorHeightMm(sensorFormat);
            var radians = 2f * Mathf.Atan(sensorHeight / (2f * Mathf.Max(focalLengthMm, 1f)));
            return radians * Mathf.Rad2Deg;
        }
    }
}
