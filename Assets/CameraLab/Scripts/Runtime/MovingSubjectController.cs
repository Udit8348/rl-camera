using UnityEngine;

namespace CameraLab.Runtime
{
    public class MovingSubjectController : MonoBehaviour
    {
        [SerializeField]
        private float speedMetersPerSecond = 2f;

        [SerializeField]
        private float travelDistance = 2.5f;

        [SerializeField]
        private Vector3 moveAxis = Vector3.right;

        private Vector3 origin;

        public void Configure(float speed, float distance, Vector3 axis)
        {
            speedMetersPerSecond = speed;
            travelDistance = distance;
            moveAxis = axis.normalized;
            origin = transform.localPosition;
        }

        private void Awake()
        {
            origin = transform.localPosition;
        }

        private void Update()
        {
            if (speedMetersPerSecond <= 0.001f || travelDistance <= 0.001f)
            {
                return;
            }

            var phase = Mathf.Sin(Time.time * speedMetersPerSecond / Mathf.Max(travelDistance, 0.1f));
            transform.localPosition = origin + moveAxis * phase * travelDistance;
        }
    }
}
