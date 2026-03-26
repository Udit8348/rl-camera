using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraLab.Core.Models
{
    [Serializable]
    public class CapturedShotData
    {
        public string screenshotPath;
        public Texture2D screenshotTexture;
        public string sceneId;
        public string timestamp;
        public float aperture;
        public float shutterSpeed;
        public int iso;
        public float focalLength;
        public float focusDistance;
        public WhiteBalancePreset whiteBalancePreset;
        public bool tripodEnabled;
        public SimulationResult simulationSnapshot;
        public ShotScoreBreakdown scoreSummary;
        public List<string> coachingMessages = new List<string>();
    }
}
