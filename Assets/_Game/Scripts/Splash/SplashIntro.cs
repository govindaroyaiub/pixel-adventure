using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PixelAdventure.Splash
{
    /// <summary>
    /// Plays the High Devs pixel intro, then loads the next scene.
    /// Timeline: pixels pop in (~1.2s) → shine sweep → hold → fade to black.
    /// </summary>
    public class SplashIntro : MonoBehaviour
    {
        [SerializeField] RawImage target;
        [SerializeField] string nextScene = "MainMenu";
        [SerializeField, Range(2, 8)] int pixelScale = 4;
        [SerializeField] float shineStart = 1.5f;
        [SerializeField] float shineDuration = 0.7f;
        [SerializeField] float holdUntil = 2.8f;
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] bool scanlines = true;

        const string SeenKey = "splash_seen";
        const float PopDuration = 0.12f;
        const float ScanlineDarken = 0.72f;

        Texture2D texture;
        Color32[] buffer;
        List<LogoPixel> pixels;
        AsyncOperation loading;
        float startTime;
        bool canSkip;
        bool finished;

        void Start()
        {
            pixels = HighDevsLogo.Build();
            int w = HighDevsLogo.Width * pixelScale, h = HighDevsLogo.Height * pixelScale;
            texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            buffer = new Color32[w * h];
            target.texture = texture;

            canSkip = PlayerPrefs.GetInt(SeenKey, 0) == 1;
            startTime = Time.unscaledTime;

            if (Application.CanStreamedLevelBeLoaded(nextScene))
            {
                loading = SceneManager.LoadSceneAsync(nextScene);
                loading.allowSceneActivation = false;
            }
            else
            {
                Debug.LogWarning($"SplashIntro: scene '{nextScene}' is not in Build Settings; splash will stay on screen.");
            }
        }

        void Update()
        {
            if (finished) return;
            float t = Time.unscaledTime - startTime;
            Render(t);
            if (t >= holdUntil + fadeDuration || (canSkip && SkipPressed())) Finish();
        }

        void Finish()
        {
            finished = true;
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            if (loading != null) loading.allowSceneActivation = true;
        }

        void Render(float t)
        {
            int s = pixelScale;
            int w = HighDevsLogo.Width * s;
            float fade = t <= holdUntil ? 1f : 1f - Mathf.Clamp01((t - holdUntil) / fadeDuration);
            Color32 black = new Color32(0, 0, 0, 255), white = new Color32(255, 255, 255, 255);

            Color32 bg = Color32.Lerp(black, HighDevsLogo.Background, fade);
            for (int i = 0; i < buffer.Length; i++) buffer[i] = bg;

            float shineT = t - shineStart;
            float band = shineT / shineDuration * 220f - 30f;
            bool shining = shineT > 0f && shineT < shineDuration + 0.1f;

            foreach (LogoPixel p in pixels)
            {
                if (t < p.Delay) continue;
                float pop = Mathf.Min(1f, (t - p.Delay) / PopDuration);
                Color32 c = Color32.Lerp(white, p.Color, pop);
                if (shining)
                {
                    float d = Mathf.Abs(p.X + p.Y * 0.6f - band);
                    if (d < 5f) c = Color32.Lerp(c, white, 0.75f * (1f - d / 5f));
                }
                c = Color32.Lerp(black, c, fade);

                int baseY = (HighDevsLogo.Height - 1 - p.Y) * s; // texture origin is bottom-left
                for (int dy = 0; dy < s; dy++)
                {
                    int row = (baseY + dy) * w + p.X * s;
                    for (int dx = 0; dx < s; dx++) buffer[row + dx] = c;
                }
            }

            if (scanlines)
            {
                int h = HighDevsLogo.Height * s;
                for (int y = 0; y < h; y++)
                {
                    int topDown = h - 1 - y;
                    if (topDown % 4 >= 2) continue;
                    int row = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = buffer[row + x];
                        buffer[row + x] = new Color32(
                            (byte)(c.r * ScanlineDarken), (byte)(c.g * ScanlineDarken), (byte)(c.b * ScanlineDarken), 255);
                    }
                }
            }

            texture.SetPixels32(buffer);
            texture.Apply(false);
        }

        static bool SkipPressed()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        }

        void OnDestroy()
        {
            if (texture != null) Destroy(texture);
        }
    }
}
