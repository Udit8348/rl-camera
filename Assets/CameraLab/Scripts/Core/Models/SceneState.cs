using System;
using CameraLab.ScriptableObjects;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class SceneState
    {
        public float ambientEV = 10f;
        public float sceneColorTemperatureK = 5500f;
        public float subjectDistanceMeters = 2f;
        public float backgroundDistanceMeters = 10f;
        public float subjectSpeedMetersPerSecond = 0f;
        public float shakeDifficulty = 1f;
        public ChallengeConfig challengeConfig;

        public SceneState Clone()
        {
            return new SceneState
            {
                ambientEV = ambientEV,
                sceneColorTemperatureK = sceneColorTemperatureK,
                subjectDistanceMeters = subjectDistanceMeters,
                backgroundDistanceMeters = backgroundDistanceMeters,
                subjectSpeedMetersPerSecond = subjectSpeedMetersPerSecond,
                shakeDifficulty = shakeDifficulty,
                challengeConfig = challengeConfig
            };
        }
    }
}
