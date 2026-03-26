using CameraLab.Core.Models;
using CameraLab.Core.Services;
using NUnit.Framework;

namespace CameraLab.Tests.EditMode
{
    public class ShotSimulationServiceTests
    {
        private ShotSimulationService simulationService;
        private SceneState baseSceneState;
        private CameraState baseCameraState;

        [SetUp]
        public void SetUp()
        {
            simulationService = new ShotSimulationService(new SimulationTuning());
            baseSceneState = new SceneState
            {
                ambientEV = 10f,
                sceneColorTemperatureK = 5500f,
                subjectDistanceMeters = 3f,
                backgroundDistanceMeters = 10f,
                subjectSpeedMetersPerSecond = 0f,
                shakeDifficulty = 1f
            };
            baseCameraState = new CameraState
            {
                apertureF = 5.6f,
                shutterSeconds = 1f / 125f,
                iso = 200,
                focalLengthMm = 50f,
                focusDistanceMeters = 3f,
                whiteBalancePreset = WhiteBalancePreset.Daylight,
                tripodEnabled = false
            };
        }

        [Test]
        public void WiderApertureProducesBrighterExposureAndMoreBackgroundBlur()
        {
            var narrow = baseCameraState.Clone();
            narrow.apertureF = 8f;
            var wide = baseCameraState.Clone();
            wide.apertureF = 2.8f;

            var narrowResult = simulationService.Simulate(narrow, baseSceneState);
            var wideResult = simulationService.Simulate(wide, baseSceneState);

            Assert.That(wideResult.exposureOffsetStops, Is.LessThan(narrowResult.exposureOffsetStops));
            Assert.That(wideResult.dofBlurStrength, Is.GreaterThan(narrowResult.dofBlurStrength));
        }

        [Test]
        public void SlowerShutterBrightensExposureButAddsMotionBlur()
        {
            var slow = baseCameraState.Clone();
            slow.shutterSeconds = 1f / 30f;
            var fast = baseCameraState.Clone();
            fast.shutterSeconds = 1f / 500f;
            baseSceneState.subjectSpeedMetersPerSecond = 4f;

            var slowResult = simulationService.Simulate(slow, baseSceneState);
            var fastResult = simulationService.Simulate(fast, baseSceneState);

            Assert.That(slowResult.exposureOffsetStops, Is.LessThan(fastResult.exposureOffsetStops));
            Assert.That(slowResult.subjectMotionBlur01, Is.GreaterThan(fastResult.subjectMotionBlur01));
        }

        [Test]
        public void HigherIsoBrightensExposureAndIncreasesNoise()
        {
            var lowIso = baseCameraState.Clone();
            lowIso.iso = 100;
            var highIso = baseCameraState.Clone();
            highIso.iso = 3200;

            var lowResult = simulationService.Simulate(lowIso, baseSceneState);
            var highResult = simulationService.Simulate(highIso, baseSceneState);

            Assert.That(highResult.exposureOffsetStops, Is.LessThan(lowResult.exposureOffsetStops));
            Assert.That(highResult.noiseAmount01, Is.GreaterThan(lowResult.noiseAmount01));
        }

        [Test]
        public void FocusErrorReducesSubjectSharpness()
        {
            var accurate = baseCameraState.Clone();
            accurate.focusDistanceMeters = baseSceneState.subjectDistanceMeters;
            var missed = baseCameraState.Clone();
            missed.focusDistanceMeters = 1f;

            var accurateResult = simulationService.Simulate(accurate, baseSceneState);
            var missedResult = simulationService.Simulate(missed, baseSceneState);

            Assert.That(accurateResult.focusSharpness01, Is.GreaterThan(missedResult.focusSharpness01));
            Assert.That(missedResult.totalBlur01, Is.GreaterThan(accurateResult.totalBlur01));
        }

        [Test]
        public void CorrectWhiteBalanceProducesLowerMismatch()
        {
            baseSceneState.sceneColorTemperatureK = 3200f;
            var daylight = baseCameraState.Clone();
            daylight.whiteBalancePreset = WhiteBalancePreset.Daylight;
            var tungsten = baseCameraState.Clone();
            tungsten.whiteBalancePreset = WhiteBalancePreset.Tungsten;

            var daylightResult = simulationService.Simulate(daylight, baseSceneState);
            var tungstenResult = simulationService.Simulate(tungsten, baseSceneState);

            Assert.That(tungstenResult.whiteBalanceMismatch01, Is.LessThan(daylightResult.whiteBalanceMismatch01));
        }
    }
}
