using CameraLab.Core.Controllers;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;

namespace CameraLab.Runtime
{
    public class CameraViewController : MonoBehaviour
    {
        [SerializeField]
        private Camera previewCamera;

        [SerializeField]
        private int previewWidth = 1280;

        [SerializeField]
        private int previewHeight = 720;

        private CameraPreset cameraPreset;
        private HandheldPreviewJitter previewJitter;
        private RenderTexture previewTexture;

        public Camera PreviewCamera => previewCamera;

        public RenderTexture PreviewTexture => previewTexture;

        public HandheldPreviewJitter PreviewJitter => previewJitter;

        public void Initialize(CameraStateController cameraStateController, CameraPreset preset)
        {
            cameraPreset = preset;
            EnsureCamera();
            EnsurePreviewTexture();

            previewJitter = previewCamera.GetComponent<HandheldPreviewJitter>();
            if (previewJitter == null)
            {
                previewJitter = previewCamera.gameObject.AddComponent<HandheldPreviewJitter>();
            }

            cameraStateController.StateChanged += HandleStateChanged;
            HandleStateChanged(cameraStateController.CurrentState);
        }

        private void EnsureCamera()
        {
            if (previewCamera != null)
            {
                return;
            }

            var cameraObject = new GameObject("SimulationCamera");
            cameraObject.transform.SetParent(transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 250f;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.allowHDR = true;
        }

        private void EnsurePreviewTexture()
        {
            if (previewTexture == null)
            {
                previewTexture = new RenderTexture(previewWidth, previewHeight, 24)
                {
                    name = "CameraLabPreview",
                    antiAliasing = 2
                };
            }

            previewCamera.targetTexture = previewTexture;
        }

        private void HandleStateChanged(Core.Models.CameraState state)
        {
            previewCamera.fieldOfView = CameraMathUtility.CalculateVerticalFieldOfView(state.focalLengthMm, cameraPreset.sensorFormat);
        }
    }
}
