using System.Collections;
using UnityEngine;

namespace PixelAdventure.Effects
{
    /// <summary>Freezes time for a few milliseconds when a hit lands, for impact.</summary>
    public class HitPause : MonoBehaviour
    {
        static HitPause instance;
        float resumeAt;
        bool running;

        public static void Trigger(float seconds)
        {
            if (instance == null)
            {
                var go = new GameObject("HitPause") { hideFlags = HideFlags.HideInHierarchy };
                instance = go.AddComponent<HitPause>();
            }
            instance.Begin(seconds);
        }

        void Begin(float seconds)
        {
            resumeAt = Mathf.Max(resumeAt, Time.unscaledTime + seconds);
            if (!running) StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            running = true;
            Time.timeScale = 0f;
            while (Time.unscaledTime < resumeAt) yield return null;
            Time.timeScale = 1f;
            running = false;
        }

        void OnDestroy()
        {
            if (running) Time.timeScale = 1f;
        }
    }
}
