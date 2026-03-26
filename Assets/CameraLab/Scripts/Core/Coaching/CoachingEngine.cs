using System.Collections.Generic;
using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;

namespace CameraLab.Core.Coaching
{
    public class CoachingEngine
    {
        public IReadOnlyList<string> GenerateMessages(CameraState cameraState, SceneState sceneState, SimulationResult result, ChallengeConfig challengeConfig)
        {
            var messages = new List<string>();

            if (result.exposureOffsetStops > 0.5f)
            {
                messages.Add("The image is underexposed. Open the aperture, slow the shutter, or raise ISO.");
            }
            else if (result.exposureOffsetStops < -0.5f)
            {
                messages.Add("The image is overexposed. Narrow the aperture, use a faster shutter, or lower ISO.");
            }

            if (result.focusSharpness01 < 0.65f)
            {
                var direction = cameraState.focusDistanceMeters < sceneState.subjectDistanceMeters ? "farther" : "closer";
                messages.Add($"The subject is out of focus. Move the focus distance {direction} to match the subject.");
            }
            else if (result.subjectMotionBlur01 > 0.35f)
            {
                messages.Add("The subject is blurred by motion. Use a faster shutter speed.");
            }
            else if (!cameraState.tripodEnabled && result.cameraShakeBlur01 > 0.3f)
            {
                messages.Add("Handheld shake is softening the image. Use a faster shutter, shorter focal length, or switch on the tripod.");
            }

            if (result.noiseAmount01 > 0.45f)
            {
                messages.Add("The image is noisy because ISO is high and the shot is being pushed from a dark exposure.");
            }

            if (result.whiteBalanceMismatch01 > 0.2f)
            {
                var selectedKelvin = CameraLab.Utilities.ColorTemperatureUtility.GetWhiteBalanceTargetKelvin(cameraState.whiteBalancePreset, sceneState.sceneColorTemperatureK);
                var tooWarm = selectedKelvin > sceneState.sceneColorTemperatureK;
                messages.Add(tooWarm
                    ? "The white balance is too warm for this scene. Choose a cooler preset."
                    : "The white balance is too cool for this scene. Choose a warmer preset.");
            }

            if (challengeConfig != null && challengeConfig.targetBlurGoal == BlurGoal.BlurBackground && result.dofBlurStrength < 0.45f)
            {
                messages.Add("The background is too sharp. Use a wider aperture, longer focal length, or more subject-background separation.");
            }

            if (messages.Count == 0)
            {
                messages.Add("The shot is balanced overall. Try trading one setting for another to compare blur, noise, and depth of field.");
            }

            if (messages.Count > 3)
            {
                messages.RemoveRange(3, messages.Count - 3);
            }

            return messages;
        }
    }
}
