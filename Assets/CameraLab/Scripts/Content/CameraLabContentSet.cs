using System.Collections.Generic;
using CameraLab.ScriptableObjects;

namespace CameraLab.Content
{
    public class CameraLabContentSet
    {
        public CameraPreset cameraPreset;
        public List<SceneConfig> scenes = new List<SceneConfig>();
        public List<ChallengeConfig> challenges = new List<ChallengeConfig>();
    }
}
