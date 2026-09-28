using PixelAdventure.Player;
using UnityEngine;

namespace PixelAdventure.Stage
{
    /// <summary>
    /// Metal Slug camera: follows the player to the right, never scrolls back,
    /// and its left edge is a wall. Also does screen shake.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class StageCamera : MonoBehaviour
    {
        [SerializeField] PlayerMotor target;
        [SerializeField] float lead = 2f;
        [SerializeField] float followSpeed = 6f;
        [SerializeField] float levelMinX = -15f;
        [SerializeField] float levelMaxX = 172f;

        static StageCamera instance;

        Camera cam;
        Vector3 basePos;
        float shakeAmplitude;
        float shakeTimeLeft;
        float shakeDuration;

        public float HalfWidth => cam.orthographicSize * cam.aspect;

        void Awake()
        {
            instance = this;
            cam = GetComponent<Camera>();
            basePos = transform.position;
        }

        public static void Shake(float amplitude, float duration)
        {
            if (instance == null) return;
            instance.shakeAmplitude = Mathf.Max(instance.shakeAmplitude * (instance.shakeTimeLeft > 0f ? 1f : 0f), amplitude);
            instance.shakeDuration = duration;
            instance.shakeTimeLeft = duration;
        }

        void LateUpdate()
        {
            if (target == null) return;
            float halfW = HalfWidth;
            float desired = target.transform.position.x + lead;
            float x = basePos.x;
            if (desired > x) x = Mathf.Lerp(x, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            x = Mathf.Clamp(x, levelMinX + halfW, Mathf.Max(levelMinX + halfW, levelMaxX - halfW));
            basePos.x = x;
            target.LeftLimit = x - halfW;

            Vector3 offset = Vector3.zero;
            if (shakeTimeLeft > 0f)
            {
                shakeTimeLeft -= Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(shakeTimeLeft / shakeDuration);
                offset = (Vector3)(Random.insideUnitCircle * shakeAmplitude * k);
            }
            transform.position = basePos + offset;
        }
    }
}
