using PixelAdventure.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelAdventure.Controls
{
    /// <summary>Round on-screen action button with a pixel label and a radial cooldown overlay.</summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] string label = "ATK";
        [SerializeField] RawImage labelImage;
        [SerializeField] Image cooldownFill;
        [SerializeField] float labelPixelSize = 6f;

        bool pressed;
        Texture2D labelTex;

        void Start()
        {
            if (labelImage == null) return;
            labelTex = PixelFont.Render(label, new[] { PixelFont.Hex("#ffffff") });
            labelImage.texture = labelTex;
            labelImage.rectTransform.sizeDelta = new Vector2(labelTex.width, labelTex.height) * labelPixelSize;
        }

        public void OnPointerDown(PointerEventData e)
        {
            pressed = true;
            transform.localScale = Vector3.one * 0.9f;
        }

        public void OnPointerUp(PointerEventData e) => transform.localScale = Vector3.one;

        /// <summary>True once per tap.</summary>
        public bool ConsumePress()
        {
            bool p = pressed;
            pressed = false;
            return p;
        }

        public void SetCooldown(float remaining01)
        {
            if (cooldownFill != null) cooldownFill.fillAmount = remaining01;
        }

        void OnDestroy()
        {
            if (labelTex != null) Destroy(labelTex);
        }
    }
}
