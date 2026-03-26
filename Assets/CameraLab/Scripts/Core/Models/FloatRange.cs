using System;
using UnityEngine;

namespace CameraLab.Core.Models
{
    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(float value)
        {
            return value >= min && value <= max;
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, min, max);
        }

        public float DistanceFrom(float value)
        {
            if (Contains(value))
            {
                return 0f;
            }

            return value < min ? min - value : value - max;
        }

        public float Normalize(float value)
        {
            if (Mathf.Approximately(min, max))
            {
                return 0f;
            }

            return Mathf.InverseLerp(min, max, value);
        }
    }
}
