using UnityEngine;

namespace CameraLab.Runtime
{
    public class HandheldPreviewJitter : MonoBehaviour
    {
        [SerializeField]
        private float positionAmplitude = 0.015f;

        [SerializeField]
        private float rotationAmplitude = 0.45f;

        [SerializeField]
        private float frequency = 8f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float shakeRisk;
        private bool tripodEnabled;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseLocalRotation = transform.localRotation;
        }

        public void SetShake(float amount, bool tripod)
        {
            shakeRisk = Mathf.Clamp01(amount);
            tripodEnabled = tripod;
        }

        private void LateUpdate()
        {
            if (tripodEnabled || shakeRisk <= 0.01f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, baseLocalPosition, 0.2f);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, baseLocalRotation, 0.2f);
                return;
            }

            var t = Time.unscaledTime * frequency;
            var offset = new Vector3(
                (Mathf.PerlinNoise(t, 0.1f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(0.3f, t) - 0.5f) * 2f,
                0f) * positionAmplitude * shakeRisk;
            var rotation = Quaternion.Euler(
                (Mathf.PerlinNoise(t, 2.1f) - 0.5f) * rotationAmplitude * shakeRisk,
                (Mathf.PerlinNoise(4.2f, t) - 0.5f) * rotationAmplitude * shakeRisk,
                (Mathf.PerlinNoise(t, 6.4f) - 0.5f) * rotationAmplitude * shakeRisk);

            transform.localPosition = baseLocalPosition + offset;
            transform.localRotation = baseLocalRotation * rotation;
        }
    }
}
