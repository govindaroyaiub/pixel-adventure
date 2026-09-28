using UnityEngine;

namespace PixelAdventure.Effects
{
    /// <summary>Fading ghost copy of a sprite, used for roll and dash trails.</summary>
    public class AfterImage : MonoBehaviour
    {
        SpriteRenderer sr;
        float life;
        float age;
        Color startColor;

        public static void Spawn(SpriteRenderer source, Color tint, float life = 0.2f)
        {
            var go = new GameObject("AfterImage");
            go.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            go.transform.localScale = source.transform.lossyScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = source.sprite;
            sr.flipX = source.flipX;
            sr.sortingOrder = source.sortingOrder - 1;
            sr.color = tint;
            var img = go.AddComponent<AfterImage>();
            img.sr = sr;
            img.life = life;
            img.startColor = tint;
        }

        void Update()
        {
            age += Time.deltaTime;
            float k = 1f - age / life;
            if (k <= 0f) { Destroy(gameObject); return; }
            sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * k);
        }
    }
}
