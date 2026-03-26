using CameraLab.Core.Models;
using CameraLab.Utilities;
using UnityEngine;

namespace CameraLab.Core.Services
{
    public class ShotSimulationService
    {
        private readonly SimulationTuning tuning;

        public ShotSimulationService(SimulationTuning tuning)
        {
            this.tuning = tuning ?? new SimulationTuning();
        }

        public SimulationResult Simulate(CameraState cameraState, SceneState sceneState)
        {
            var result = new SimulationResult();

            var cameraEV100 = CameraMathUtility.Log2((cameraState.apertureF * cameraState.apertureF) / cameraState.shutterSeconds);
            var cameraEV = cameraEV100 - CameraMathUtility.Log2(cameraState.iso / 100f);

            // Positive values mean the camera is letting in less light than the scene needs.
            result.exposureOffsetStops = cameraEV - sceneState.ambientEV;
            result.exposureNormalized01 = Mathf.Clamp01(
                1f - Mathf.Abs(result.exposureOffsetStops) / Mathf.Max(tuning.maxExposurePenaltyStops, 0.0001f));

            var focusTolerance = CalculateFocusTolerance(cameraState, sceneState);
            result.focusErrorMeters = Mathf.Abs(cameraState.focusDistanceMeters - sceneState.subjectDistanceMeters);
            result.focusSharpness01 = 1f / (1f + Mathf.Pow(result.focusErrorMeters / Mathf.Max(focusTolerance, 0.0001f), 2f));

            var apertureFactor = CameraMathUtility.InverseRemap01(cameraState.apertureF, 1.8f, 16f);
            var focalLengthFactor = CameraMathUtility.Remap01(cameraState.focalLengthMm, 24f, 120f);
            var subjectDistanceFactor = CameraMathUtility.InverseRemap01(sceneState.subjectDistanceMeters, 0.75f, 6f);
            var separation = Mathf.Max(0f, sceneState.backgroundDistanceMeters - sceneState.subjectDistanceMeters);
            var backgroundSeparationFactor = CameraMathUtility.Remap01(separation, 0.5f, tuning.dofSeparationDistanceMax);
            var focusPlacementFactor = Mathf.Lerp(0.45f, 1f, result.focusSharpness01);

            result.dofBlurStrength = Mathf.Clamp01(
                apertureFactor * 0.4f +
                focalLengthFactor * 0.25f +
                subjectDistanceFactor * 0.15f +
                backgroundSeparationFactor * 0.2f);
            result.dofBlurStrength *= focusPlacementFactor;

            var motionExposure = cameraState.shutterSeconds * sceneState.subjectSpeedMetersPerSecond;
            result.subjectMotionBlur01 = Mathf.Clamp01(1f - Mathf.Exp(-motionExposure * tuning.motionBlurScalar));

            var safeHandheldShutter = 1f / Mathf.Max(1f, cameraState.focalLengthMm * tuning.safeHandheldScalar);
            var shakeRatio = (cameraState.shutterSeconds / Mathf.Max(safeHandheldShutter, 0.0001f)) * sceneState.shakeDifficulty;
            result.cameraShakeBlur01 = cameraState.tripodEnabled
                ? 0.02f * sceneState.shakeDifficulty
                : Mathf.Clamp01(1f - Mathf.Exp(-Mathf.Max(0f, shakeRatio - 0.3f) * tuning.shakeCurveScalar));

            var focusBlur01 = 1f - result.focusSharpness01;
            result.totalBlur01 = Mathf.Clamp01(
                focusBlur01 * 0.4f +
                result.subjectMotionBlur01 * 0.35f +
                result.cameraShakeBlur01 * 0.25f);

            var isoFactor = Mathf.Clamp01(CameraMathUtility.Log2(cameraState.iso / 100f) / Mathf.Max(tuning.isoNoiseStopsForMax, 0.0001f));
            var underExposurePenalty = Mathf.Clamp01(Mathf.Max(0f, result.exposureOffsetStops) / Mathf.Max(tuning.underExposureNoiseStopsForMax, 0.0001f));
            var lowLightFactor = CameraMathUtility.InverseRemap01(sceneState.ambientEV, 4f, 12f);
            result.noiseAmount01 = Mathf.Clamp01(isoFactor * 0.55f + underExposurePenalty * 0.35f + lowLightFactor * 0.1f);

            var whiteBalanceTarget = ColorTemperatureUtility.GetWhiteBalanceTargetKelvin(
                cameraState.whiteBalancePreset,
                sceneState.sceneColorTemperatureK);
            result.whiteBalanceMismatch01 = Mathf.Clamp01(
                Mathf.Abs(sceneState.sceneColorTemperatureK - whiteBalanceTarget) /
                Mathf.Max(tuning.maxWhiteBalanceMismatchKelvin, 0.0001f));

            var sharpnessScore = Mathf.Clamp01(1f - result.totalBlur01);
            var noiseScore = Mathf.Clamp01(1f - result.noiseAmount01);
            var whiteBalanceScore = Mathf.Clamp01(1f - result.whiteBalanceMismatch01);
            result.finalImageQuality01 = Mathf.Clamp01(
                result.exposureNormalized01 * 0.3f +
                sharpnessScore * 0.4f +
                noiseScore * 0.15f +
                whiteBalanceScore * 0.15f);

            result.debugNotes.Add($"CameraEV={cameraEV:0.00}");
            result.debugNotes.Add($"ExposureOffset={result.exposureOffsetStops:0.00} stops");
            result.debugNotes.Add($"FocusTolerance={focusTolerance:0.00}m");
            result.debugNotes.Add($"WBMismatch={result.whiteBalanceMismatch01:0.00}");

            return result;
        }

        private float CalculateFocusTolerance(CameraState cameraState, SceneState sceneState)
        {
            var apertureForgiveness = Mathf.Lerp(
                tuning.focusToleranceCloseMeters,
                tuning.focusToleranceFarMeters,
                CameraMathUtility.Remap01(cameraState.apertureF, 1.8f, 16f));
            var distanceForgiveness = sceneState.subjectDistanceMeters * tuning.focusDistanceInfluence;
            var focalPenalty = Mathf.Lerp(1f, tuning.focalLengthFocusPenalty, CameraMathUtility.Remap01(cameraState.focalLengthMm, 24f, 120f));
            return Mathf.Max(0.01f, (apertureForgiveness + distanceForgiveness) * focalPenalty);
        }
    }
}
