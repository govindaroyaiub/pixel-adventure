using PixelAdventure.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace PixelAdventure.UI
{
    /// <summary>HUD bar bound to a Health. Fill drains instantly; a pale "damage trail" catches up.</summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health;
        [SerializeField] Image fill;
        [SerializeField] Image trail;
        [SerializeField] RawImage label;

        float target = 1f;
        Texture2D labelTex;

        void Start()
        {
            health.Changed += (cur, max) => target = (float)cur / max;
            target = (float)health.Current / health.Max;
            fill.fillAmount = trail.fillAmount = target;

            if (label != null)
            {
                labelTex = PixelFont.Render("HP", new[] { PixelFont.Hex("#fff3a0"), PixelFont.Hex("#ffb000") });
                label.texture = labelTex;
                label.rectTransform.sizeDelta = new Vector2(labelTex.width, labelTex.height) * 6f;
            }
        }

        void Update()
        {
            fill.fillAmount = target;
            trail.fillAmount = Mathf.MoveTowards(trail.fillAmount, target, Time.unscaledDeltaTime * 0.8f);
        }

        void OnDestroy()
        {
            if (labelTex != null) Destroy(labelTex);
        }
    }
}
