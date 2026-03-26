using System;
using System.Collections;
using System.IO;
using CameraLab.Core.Models;
using UnityEngine;

namespace CameraLab.Core.Controllers
{
    public class CaptureManager : MonoBehaviour
    {
        public IEnumerator CaptureShot(
            RenderTexture sourceTexture,
            string sceneId,
            CameraState cameraState,
            SimulationResult result,
            ShotScoreBreakdown scoreBreakdown,
            System.Collections.Generic.IReadOnlyList<string> coachingMessages,
            Action<CapturedShotData> onCaptured)
        {
            yield return new WaitForEndOfFrame();

            var previous = RenderTexture.active;
            RenderTexture.active = sourceTexture;
            var texture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, sourceTexture.width, sourceTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            var capturesDirectory = Path.Combine(Application.persistentDataPath, "CameraLabCaptures");
            Directory.CreateDirectory(capturesDirectory);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var path = Path.Combine(capturesDirectory, $"{sceneId}_{timestamp}.png");
            File.WriteAllBytes(path, texture.EncodeToPNG());

            var capturedShot = new CapturedShotData
            {
                screenshotPath = path,
                screenshotTexture = texture,
                sceneId = sceneId,
                timestamp = timestamp,
                aperture = cameraState.apertureF,
                shutterSpeed = cameraState.shutterSeconds,
                iso = cameraState.iso,
                focalLength = cameraState.focalLengthMm,
                focusDistance = cameraState.focusDistanceMeters,
                whiteBalancePreset = cameraState.whiteBalancePreset,
                tripodEnabled = cameraState.tripodEnabled,
                simulationSnapshot = result.Clone(),
                scoreSummary = scoreBreakdown.Clone(),
                coachingMessages = new System.Collections.Generic.List<string>(coachingMessages)
            };

            onCaptured?.Invoke(capturedShot);
        }
    }
}
