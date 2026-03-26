using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;

namespace CameraLab.Core.Evaluation
{
    public class ShotEvaluationService
    {
        public ShotScoreBreakdown Evaluate(SimulationResult result, ChallengeConfig challengeConfig)
        {
            var breakdown = new ShotScoreBreakdown
            {
                exposureScore01 = Mathf.Clamp01(result.exposureNormalized01),
                sharpnessScore01 = Mathf.Clamp01(1f - result.totalBlur01),
                noiseScore01 = Mathf.Clamp01(1f - result.noiseAmount01),
                whiteBalanceScore01 = Mathf.Clamp01(1f - result.whiteBalanceMismatch01)
            };

            if (challengeConfig == null)
            {
                breakdown.overallScore01 =
                    breakdown.exposureScore01 * 0.3f +
                    breakdown.sharpnessScore01 * 0.4f +
                    breakdown.noiseScore01 * 0.15f +
                    breakdown.whiteBalanceScore01 * 0.15f;
                breakdown.passFail = breakdown.overallScore01 >= 0.65f;
                breakdown.objectiveNotes.Add("Sandbox evaluation uses the baseline teaching rubric.");
                return breakdown;
            }

            breakdown.exposureScore01 = Mathf.Max(
                breakdown.exposureScore01,
                CameraMathUtility.EvaluateRangeScore(result.exposureOffsetStops, challengeConfig.targetExposureRange, 2f));

            var subjectSharpness = Mathf.Clamp01(1f - result.totalBlur01);
            breakdown.sharpnessScore01 = Mathf.Max(
                breakdown.sharpnessScore01,
                CameraMathUtility.EvaluateRangeScore(subjectSharpness, challengeConfig.targetSharpnessRange, 0.35f));

            var wbAccuracy = Mathf.Clamp01(1f - result.whiteBalanceMismatch01);
            var wbRangeValue = result.whiteBalanceMismatch01;
            breakdown.whiteBalanceScore01 = Mathf.Max(
                wbAccuracy,
                CameraMathUtility.EvaluateRangeScore(wbRangeValue, challengeConfig.targetWhiteBalanceRange, 0.3f));

            var blurGoalScore = EvaluateBlurGoal(result, challengeConfig, breakdown);
            var weights = challengeConfig.scoringWeights;
            var weightTotal = Mathf.Max(0.0001f, weights.exposureWeight + weights.sharpnessWeight + weights.noiseWeight + weights.whiteBalanceWeight);
            breakdown.overallScore01 = Mathf.Clamp01(
                (breakdown.exposureScore01 * weights.exposureWeight +
                 Mathf.Max(breakdown.sharpnessScore01, blurGoalScore) * weights.sharpnessWeight +
                 breakdown.noiseScore01 * weights.noiseWeight +
                 breakdown.whiteBalanceScore01 * weights.whiteBalanceWeight) / weightTotal);

            breakdown.passFail = breakdown.overallScore01 >= challengeConfig.minimumPassScore && blurGoalScore >= 0.55f;
            return breakdown;
        }

        private float EvaluateBlurGoal(SimulationResult result, ChallengeConfig challengeConfig, ShotScoreBreakdown breakdown)
        {
            switch (challengeConfig.targetBlurGoal)
            {
                case BlurGoal.BlurBackground:
                    {
                        var score = Mathf.Clamp01(result.dofBlurStrength * 0.6f + result.focusSharpness01 * 0.4f);
                        breakdown.objectiveNotes.Add(score >= 0.65f
                            ? "Background blur target met."
                            : "Background separation is still too weak for the brief.");
                        return score;
                    }
                case BlurGoal.KeepBackgroundSharp:
                    {
                        var score = Mathf.Clamp01((1f - result.dofBlurStrength) * 0.5f + result.focusSharpness01 * 0.5f);
                        breakdown.objectiveNotes.Add(score >= 0.65f
                            ? "Background stayed relatively sharp."
                            : "Depth of field is shallower than this challenge wants.");
                        return score;
                    }
                case BlurGoal.FreezeMotion:
                    {
                        var score = Mathf.Clamp01(1f - result.subjectMotionBlur01);
                        breakdown.objectiveNotes.Add(score >= 0.7f
                            ? "Motion is controlled."
                            : "The subject still shows motion blur.");
                        return score;
                    }
                case BlurGoal.AcceptSomeMotion:
                    {
                        var score = Mathf.Clamp01(1f - Mathf.Abs(result.subjectMotionBlur01 - 0.45f) / 0.45f);
                        breakdown.objectiveNotes.Add(score >= 0.6f
                            ? "The shot keeps a readable sense of motion."
                            : "The motion treatment does not match the brief yet.");
                        return score;
                    }
                default:
                    breakdown.objectiveNotes.Add("No special blur objective was active.");
                    return breakdown.sharpnessScore01;
            }
        }
    }
}
