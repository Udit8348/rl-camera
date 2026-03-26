using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.Utilities
{
    public static class ColorTemperatureUtility
    {
        public static float GetWhiteBalanceTargetKelvin(WhiteBalancePreset preset, float sceneTemperatureK)
        {
            switch (preset)
            {
                case WhiteBalancePreset.Daylight:
                    return 5500f;
                case WhiteBalancePreset.Cloudy:
                    return 6500f;
                case WhiteBalancePreset.Shade:
                    return 7500f;
                case WhiteBalancePreset.Tungsten:
                    return 3200f;
                case WhiteBalancePreset.Fluorescent:
                    return 4200f;
                default:
                    return sceneTemperatureK;
            }
        }

        public static Color KelvinToColor(float kelvin)
        {
            var temperature = Mathf.Clamp(kelvin / 100f, 10f, 400f);

            float red;
            float green;
            float blue;

            if (temperature <= 66f)
            {
                red = 255f;
                green = 99.4708f * Mathf.Log(temperature) - 161.11957f;
                blue = temperature <= 19f ? 0f : 138.51773f * Mathf.Log(temperature - 10f) - 305.0448f;
            }
            else
            {
                red = 329.69873f * Mathf.Pow(temperature - 60f, -0.13320476f);
                green = 288.12216f * Mathf.Pow(temperature - 60f, -0.075514846f);
                blue = 255f;
            }

            return new Color(
                Mathf.Clamp01(red / 255f),
                Mathf.Clamp01(green / 255f),
                Mathf.Clamp01(blue / 255f));
        }
    }
}
