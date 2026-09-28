using UnityEngine;

namespace PixelAdventure.Core
{
    /// <summary>Time-based cooldown. Pass the current time in so it stays testable.</summary>
    public class Cooldown
    {
        public float Duration;
        float readyAt = float.MinValue;

        public Cooldown(float duration) { Duration = duration; }

        public bool IsReady(float now) => now >= readyAt;
        public void Trigger(float now) => readyAt = now + Duration;
        public void Reset() => readyAt = float.MinValue;

        /// <summary>1 right after triggering, 0 when ready. Drives the button cooldown sweep.</summary>
        public float Remaining01(float now) => Duration <= 0f ? 0f : Mathf.Clamp01((readyAt - now) / Duration);
    }
}
