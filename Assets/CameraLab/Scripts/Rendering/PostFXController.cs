using CameraLab.Core.Models;
using CameraLab.Runtime;
using CameraLab.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CameraLab.Rendering
{
    public class PostFXController : MonoBehaviour
    {
        private ColorAdjustments colorAdjustments;
        private WhiteBalance whiteBalance;
        private DepthOfField depthOfField;
        private MotionBlur motionBlur;
        private FilmGrain filmGrain;
        private HandheldPreviewJitter previewJitter;

        public void Initialize(Transform parent, HandheldPreviewJitter jitter)
        {
            previewJitter = jitter;

            var volumeObject = new GameObject("CameraLabGlobalVolume");
            volumeObject.transform.SetParent(parent, false);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            colorAdjustments = volume.profile.Add<ColorAdjustments>(true);
            whiteBalance = volume.profile.Add<WhiteBalance>(true);
            depthOfField = volume.profile.Add<DepthOfField>(true);
            motionBlur = volume.profile.Add<MotionBlur>(true);
            filmGrain = volume.profile.Add<FilmGrain>(true);

            depthOfField.mode.Override(DepthOfFieldMode.Bokeh);
            depthOfField.highQualitySampling.Override(true);
            depthOfField.bladeCount.Override(5);
            depthOfField.bladeCurvature.Override(0.9f);
            motionBlur.mode.Override(MotionBlurMode.CameraAndObjects);
            motionBlur.quality.Override(MotionBlurQuality.Medium);
            motionBlur.clamp.Override(0.08f);
            filmGrain.type.Override(FilmGrainLookup.Thin1);
            filmGrain.response.Override(0.78f);
        }

        public void Apply(CameraState cameraState, SceneState sceneState, SimulationResult result)
        {
            colorAdjustments.postExposure.Override(Mathf.Clamp(-result.exposureOffsetStops, -4f, 4f));
            colorAdjustments.contrast.Override(Mathf.Lerp(-8f, 10f, result.exposureNormalized01));

            var selectedKelvin = ColorTemperatureUtility.GetWhiteBalanceTargetKelvin(cameraState.whiteBalancePreset, sceneState.sceneColorTemperatureK);
            var temperatureDelta = Mathf.Clamp((selectedKelvin - sceneState.sceneColorTemperatureK) / 35f, -100f, 100f);
            whiteBalance.temperature.Override(temperatureDelta);
            whiteBalance.tint.Override(0f);

            depthOfField.focusDistance.Override(Mathf.Max(0.1f, cameraState.focusDistanceMeters));
            depthOfField.aperture.Override(cameraState.apertureF);
            depthOfField.focalLength.Override(cameraState.focalLengthMm);
            depthOfField.active = result.dofBlurStrength > 0.05f;

            motionBlur.intensity.Override(Mathf.Clamp01(Mathf.Max(result.subjectMotionBlur01, result.cameraShakeBlur01)));
            filmGrain.intensity.Override(Mathf.Clamp01(result.noiseAmount01));
            previewJitter.SetShake(result.cameraShakeBlur01, cameraState.tripodEnabled);
        }
    }
}
