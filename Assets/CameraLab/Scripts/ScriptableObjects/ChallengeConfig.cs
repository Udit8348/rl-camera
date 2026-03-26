using System.Collections.Generic;
using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CameraLab/Challenge Config", fileName = "ChallengeConfig")]
    public class ChallengeConfig : ScriptableObject
    {
        public string challengeId;
        public string displayName;
        [TextArea]
        public string objectiveText;
        public FloatRange targetExposureRange = new FloatRange(-0.5f, 0.5f);
        public FloatRange targetSharpnessRange = new FloatRange(0.65f, 1f);
        public BlurGoal targetBlurGoal = BlurGoal.Neutral;
        public FloatRange targetWhiteBalanceRange = new FloatRange(0f, 0.2f);
        public ShotScoringWeights scoringWeights = ShotScoringWeights.Default;
        [Range(0.1f, 1f)]
        public float minimumPassScore = 0.7f;
        public List<string> coachingHints = new List<string>();
    }
}
