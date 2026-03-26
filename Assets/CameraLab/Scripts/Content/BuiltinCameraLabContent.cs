using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using UnityEngine;

namespace CameraLab.Content
{
    public static class BuiltinCameraLabContent
    {
        public static CameraLabContentSet Create()
        {
            var content = new CameraLabContentSet();
            content.cameraPreset = CreateCameraPreset();

            var portraitChallenge = CreateChallenge(
                "challenge_portrait",
                "Blurred Portrait",
                "Create a bright, sharp portrait with a blurred background.",
                new FloatRange(-0.35f, 0.35f),
                new FloatRange(0.68f, 1f),
                BlurGoal.BlurBackground,
                new FloatRange(0f, 0.2f),
                0.72f);

            var tungstenChallenge = CreateChallenge(
                "challenge_tungsten",
                "Neutralize Tungsten Light",
                "Correct the warm color cast and keep the subject usable in this low-light room.",
                new FloatRange(-0.5f, 0.5f),
                new FloatRange(0.6f, 1f),
                BlurGoal.Neutral,
                new FloatRange(0f, 0.12f),
                0.68f);

            var sportsChallenge = CreateChallenge(
                "challenge_sports",
                "Freeze The Runner",
                "Freeze the moving subject while keeping the exposure usable.",
                new FloatRange(-0.5f, 0.5f),
                new FloatRange(0.65f, 1f),
                BlurGoal.FreezeMotion,
                new FloatRange(0f, 0.25f),
                0.7f);

            content.challenges.Add(portraitChallenge);
            content.challenges.Add(tungstenChallenge);
            content.challenges.Add(sportsChallenge);

            content.scenes.Add(CreateScene(
                "outdoor_portrait",
                "Outdoor Portrait Daylight",
                "Bright daylight portrait scene for exposure and background blur fundamentals.",
                14f,
                5500f,
                2f,
                16f,
                0f,
                0.85f,
                portraitChallenge,
                new SceneCameraDefaults
                {
                    apertureF = 5.6f,
                    shutterSeconds = 1f / 250f,
                    iso = 100,
                    focalLengthMm = 50f,
                    focusDistanceMeters = 2f,
                    whiteBalancePreset = WhiteBalancePreset.Daylight,
                    tripodEnabled = false
                }));

            content.scenes.Add(CreateScene(
                "indoor_tungsten",
                "Indoor Tungsten Room",
                "Warm indoor room that teaches white balance and low-light tradeoffs.",
                7f,
                3200f,
                3f,
                7f,
                0f,
                1.15f,
                tungstenChallenge,
                new SceneCameraDefaults
                {
                    apertureF = 4f,
                    shutterSeconds = 1f / 60f,
                    iso = 400,
                    focalLengthMm = 50f,
                    focusDistanceMeters = 3f,
                    whiteBalancePreset = WhiteBalancePreset.Daylight,
                    tripodEnabled = false
                }));

            content.scenes.Add(CreateScene(
                "sports_runner",
                "Moving Subject / Sports",
                "Outdoor action setup that highlights shutter speed, framing, and shake sensitivity.",
                12f,
                6000f,
                10f,
                24f,
                4f,
                1.25f,
                sportsChallenge,
                new SceneCameraDefaults
                {
                    apertureF = 5.6f,
                    shutterSeconds = 1f / 60f,
                    iso = 200,
                    focalLengthMm = 85f,
                    focusDistanceMeters = 10f,
                    whiteBalancePreset = WhiteBalancePreset.Daylight,
                    tripodEnabled = false
                }));

            return content;
        }

        private static CameraPreset CreateCameraPreset()
        {
            var preset = ScriptableObject.CreateInstance<CameraPreset>();
            preset.hideFlags = HideFlags.DontSave;
            preset.name = "BuiltInTrainingCamera";
            preset.presetName = "CameraLab Training Camera";
            preset.sensorFormat = SensorFormat.FullFrame;
            preset.minAperture = 1.8f;
            preset.maxAperture = 16f;
            preset.minShutter = 1f / 2000f;
            preset.maxShutter = 1f;
            preset.minISO = 100;
            preset.maxISO = 6400;
            preset.minFocalLength = 24f;
            preset.maxFocalLength = 120f;
            return preset;
        }

        private static ChallengeConfig CreateChallenge(
            string id,
            string name,
            string objective,
            FloatRange exposureRange,
            FloatRange sharpnessRange,
            BlurGoal blurGoal,
            FloatRange whiteBalanceRange,
            float passScore)
        {
            var challenge = ScriptableObject.CreateInstance<ChallengeConfig>();
            challenge.hideFlags = HideFlags.DontSave;
            challenge.name = name;
            challenge.challengeId = id;
            challenge.displayName = name;
            challenge.objectiveText = objective;
            challenge.targetExposureRange = exposureRange;
            challenge.targetSharpnessRange = sharpnessRange;
            challenge.targetBlurGoal = blurGoal;
            challenge.targetWhiteBalanceRange = whiteBalanceRange;
            challenge.minimumPassScore = passScore;
            challenge.coachingHints.Add("Balance the shot for the brief, not just for brightness.");
            return challenge;
        }

        private static SceneConfig CreateScene(
            string id,
            string name,
            string description,
            float ambientEV,
            float colorTemperatureK,
            float subjectDistance,
            float backgroundDistance,
            float subjectSpeed,
            float shakeDifficulty,
            ChallengeConfig challenge,
            SceneCameraDefaults defaults)
        {
            var scene = ScriptableObject.CreateInstance<SceneConfig>();
            scene.hideFlags = HideFlags.DontSave;
            scene.name = name;
            scene.sceneId = id;
            scene.displayName = name;
            scene.shortDescription = description;
            scene.ambientEV = ambientEV;
            scene.sceneColorTemperatureK = colorTemperatureK;
            scene.subjectDistanceMeters = subjectDistance;
            scene.backgroundDistanceMeters = backgroundDistance;
            scene.subjectSpeedMetersPerSecond = subjectSpeed;
            scene.shakeDifficulty = shakeDifficulty;
            scene.challengeConfig = challenge;
            scene.defaultCameraSpawnTransform = new SceneSpawnTransformData
            {
                position = new Vector3(0f, 1.6f, -0.5f),
                eulerAngles = Vector3.zero
            };
            scene.defaultCameraSettings = defaults;
            return scene;
        }
    }
}
