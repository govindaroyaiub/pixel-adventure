using UnityEngine;

namespace PixelAdventure.Effects
{
    /// <summary>Expanding, fading ring (Holy Slam impact).</summary>
    public class Shockwave : MonoBehaviour
    {
        SpriteRenderer sr;
        float radius;
        float duration;
        float age;
        Color color;

        public static void Spawn(Sprite ring, Vector2 position, float radius, Color color, float duration = 0.35f)
        {
            if (ring == null) return;
            var go = new GameObject("Shockwave");
            go.transform.position = position;
            var sw = go.AddComponent<Shockwave>();
            sw.sr = go.AddComponent<SpriteRenderer>();
            sw.sr.sprite = ring;
            sw.sr.sortingOrder = 20;
            sw.radius = radius;
            sw.duration = duration;
            sw.color = color;
            sw.Tick(0f);
        }

        void Update() => Tick(Time.deltaTime);

        void Tick(float dt)
        {
            age += dt;
            float t = Mathf.Clamp01(age / duration);
            float diameter = Mathf.Lerp(0.3f, radius * 2f, 1f - (1f - t) * (1f - t));
            transform.localScale = Vector3.one * (diameter / sr.sprite.bounds.size.x);
            sr.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
