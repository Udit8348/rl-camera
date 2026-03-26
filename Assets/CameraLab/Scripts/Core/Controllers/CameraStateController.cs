using System;
using System.Collections.Generic;
using CameraLab.Core.Models;
using CameraLab.ScriptableObjects;
using CameraLab.Utilities;
using UnityEngine;

namespace CameraLab.Core.Controllers
{
    public class CameraStateController : MonoBehaviour
    {
        [SerializeField]
        private CameraPreset cameraPreset;

        [SerializeField]
        private CameraState currentState = new CameraState();

        [SerializeField]
        private float minFocusDistance = 0.5f;

        [SerializeField]
        private float maxFocusDistance = 25f;

        public event Action<CameraState> StateChanged;

        public CameraState CurrentState => currentState;

        public CameraPreset CameraPreset => cameraPreset;

        public float MinFocusDistance => minFocusDistance;

        public float MaxFocusDistance => maxFocusDistance;

        public void Initialize(CameraPreset preset, CameraState initialState)
        {
            cameraPreset = preset;
            currentState = initialState?.Clone() ?? new CameraState();
            ClampState();
            NotifyChanged();
        }

        public void ConfigureFocusRange(float min, float max)
        {
            minFocusDistance = Mathf.Max(0.1f, min);
            maxFocusDistance = Mathf.Max(minFocusDistance, max);
            currentState.focusDistanceMeters = Mathf.Clamp(currentState.focusDistanceMeters, minFocusDistance, maxFocusDistance);
            NotifyChanged();
        }

        public IReadOnlyList<float> GetApertureOptions()
        {
            return CameraSettingsCatalog.GetApertureOptions(cameraPreset);
        }

        public IReadOnlyList<float> GetShutterOptions()
        {
            return CameraSettingsCatalog.GetShutterOptions(cameraPreset);
        }

        public IReadOnlyList<int> GetIsoOptions()
        {
            return CameraSettingsCatalog.GetIsoOptions(cameraPreset);
        }

        public IReadOnlyList<WhiteBalancePreset> GetWhiteBalanceOptions()
        {
            return CameraSettingsCatalog.GetWhiteBalanceOptions();
        }

        public void StepAperture(int direction)
        {
            var options = GetApertureOptions();
            var index = GetClosestIndex(options, currentState.apertureF);
            index = Mathf.Clamp(index + direction, 0, options.Count - 1);
            SetAperture(options[index]);
        }

        public void StepShutter(int direction)
        {
            var options = GetShutterOptions();
            var index = GetClosestIndex(options, currentState.shutterSeconds);
            index = Mathf.Clamp(index + direction, 0, options.Count - 1);
            SetShutter(options[index]);
        }

        public void StepIso(int direction)
        {
            var options = GetIsoOptions();
            var index = GetClosestIndex(options, currentState.iso);
            index = Mathf.Clamp(index + direction, 0, options.Count - 1);
            SetIso(options[index]);
        }

        public void StepWhiteBalance(int direction)
        {
            var options = GetWhiteBalanceOptions();
            var index = 0;
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i] == currentState.whiteBalancePreset)
                {
                    index = i;
                    break;
                }
            }
            index = Mathf.Clamp(index + direction, 0, options.Count - 1);
            SetWhiteBalance(options[index]);
        }

        public void SetAperture(float aperture)
        {
            currentState.apertureF = aperture;
            ClampState();
            NotifyChanged();
        }

        public void SetShutter(float shutterSeconds)
        {
            currentState.shutterSeconds = shutterSeconds;
            ClampState();
            NotifyChanged();
        }

        public void SetIso(int iso)
        {
            currentState.iso = iso;
            ClampState();
            NotifyChanged();
        }

        public void SetFocalLength(float focalLengthMm)
        {
            currentState.focalLengthMm = focalLengthMm;
            ClampState();
            NotifyChanged();
        }

        public void SetFocusDistance(float focusDistanceMeters)
        {
            currentState.focusDistanceMeters = focusDistanceMeters;
            ClampState();
            NotifyChanged();
        }

        public void SetWhiteBalance(WhiteBalancePreset preset)
        {
            currentState.whiteBalancePreset = preset;
            NotifyChanged();
        }

        public void SetTripodEnabled(bool tripodEnabled)
        {
            currentState.tripodEnabled = tripodEnabled;
            NotifyChanged();
        }

        public void ResetTo(CameraState state)
        {
            currentState = state.Clone();
            ClampState();
            NotifyChanged();
        }

        private void ClampState()
        {
            if (cameraPreset == null)
            {
                return;
            }

            currentState.apertureF = Mathf.Clamp(currentState.apertureF, cameraPreset.minAperture, cameraPreset.maxAperture);
            currentState.shutterSeconds = Mathf.Clamp(currentState.shutterSeconds, cameraPreset.minShutter, cameraPreset.maxShutter);
            currentState.iso = Mathf.Clamp(currentState.iso, cameraPreset.minISO, cameraPreset.maxISO);
            currentState.focalLengthMm = Mathf.Clamp(currentState.focalLengthMm, cameraPreset.minFocalLength, cameraPreset.maxFocalLength);
            currentState.focusDistanceMeters = Mathf.Clamp(currentState.focusDistanceMeters, minFocusDistance, maxFocusDistance);
        }

        private void NotifyChanged()
        {
            StateChanged?.Invoke(currentState);
        }

        private static int GetClosestIndex(IReadOnlyList<float> options, float value)
        {
            var bestIndex = 0;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < options.Count; i++)
            {
                var distance = Mathf.Abs(options[i] - value);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static int GetClosestIndex(IReadOnlyList<int> options, int value)
        {
            var bestIndex = 0;
            var bestDistance = int.MaxValue;
            for (var i = 0; i < options.Count; i++)
            {
                var distance = Mathf.Abs(options[i] - value);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }
    }
}
