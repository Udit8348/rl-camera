using System;
using UnityEngine;

namespace CameraLab.Core.Models
{
    [Serializable]
    public struct SceneSpawnTransformData
    {
        public Vector3 position;
        public Vector3 eulerAngles;

        public Pose ToPose()
        {
            return new Pose(position, Quaternion.Euler(eulerAngles));
        }
    }
}
