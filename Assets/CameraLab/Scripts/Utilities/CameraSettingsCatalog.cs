using System;
using System.Collections.Generic;
using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using UnityEngine;

namespace CameraLab.Utilities
{
    public static class CameraSettingsCatalog
    {
        private static readonly float[] ApertureValues =
        {
            1.8f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f
        };

        private static readonly float[] ShutterValues =
        {
            1f / 2000f, 1f / 1000f, 1f / 500f, 1f / 250f, 1f / 125f, 1f / 60f,
            1f / 30f, 1f / 15f, 1f / 8f, 1f / 4f, 1f / 2f, 1f
        };

        private static readonly int[] IsoValues =
        {
            100, 200, 400, 800, 1600, 3200, 6400
        };

        public static IReadOnlyList<float> GetApertureOptions(CameraPreset preset)
        {
            return Filter(ApertureValues, preset.minAperture, preset.maxAperture);
        }

        public static IReadOnlyList<float> GetShutterOptions(CameraPreset preset)
        {
            return Filter(ShutterValues, preset.minShutter, preset.maxShutter);
        }

        public static IReadOnlyList<int> GetIsoOptions(CameraPreset preset)
        {
            var list = new List<int>();
            for (var i = 0; i < IsoValues.Length; i++)
            {
                if (IsoValues[i] >= preset.minISO && IsoValues[i] <= preset.maxISO)
                {
                    list.Add(IsoValues[i]);
                }
            }

            return list;
        }

        public static IReadOnlyList<WhiteBalancePreset> GetWhiteBalanceOptions()
        {
            return (WhiteBalancePreset[])Enum.GetValues(typeof(WhiteBalancePreset));
        }

        public static string FormatAperture(float aperture)
        {
            return $"f/{aperture:0.0}";
        }

        public static string FormatShutter(float shutterSeconds)
        {
            if (shutterSeconds >= 1f)
            {
                return $"{shutterSeconds:0.#}s";
            }

            return $"1/{Mathf.RoundToInt(1f / Mathf.Max(shutterSeconds, 0.0001f))}s";
        }

        public static string FormatIso(int iso)
        {
            return $"ISO {iso}";
        }

        public static string FormatWhiteBalance(WhiteBalancePreset preset)
        {
            switch (preset)
            {
                case WhiteBalancePreset.Daylight:
                    return "Daylight";
                case WhiteBalancePreset.Cloudy:
                    return "Cloudy";
                case WhiteBalancePreset.Shade:
                    return "Shade";
                case WhiteBalancePreset.Tungsten:
                    return "Tungsten";
                case WhiteBalancePreset.Fluorescent:
                    return "Fluorescent";
                default:
                    return "Auto";
            }
        }

        private static IReadOnlyList<float> Filter(float[] source, float min, float max)
        {
            var list = new List<float>();
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] >= min && source[i] <= max)
                {
                    list.Add(source[i]);
                }
            }

            return list;
        }
    }
}
