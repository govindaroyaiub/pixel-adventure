using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PixelAdventure.UI
{
    /// <summary>
    /// Title screen: pixel "PIXEL ADVENTURE" title plus Play and Exit buttons.
    /// Labels are rendered with PixelFont at runtime so everything stays pixel-crisp.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] RawImage title;
        [SerializeField] Button playButton;
        [SerializeField] RawImage playLabel;
        [SerializeField] Button exitButton;
        [SerializeField] RawImage exitLabel;
        [SerializeField] string gameScene = "Stage1-1";
        [SerializeField] float titlePixelSize = 12f;
        [SerializeField] float labelPixelSize = 8f;

        Texture2D titleTex, playTex, exitTex;
        Vector2 titleBasePos;

        void Start()
        {
            Color32[] gold = { PixelFont.Hex("#fff3a0"), PixelFont.Hex("#ffd24d"), PixelFont.Hex("#ffb000"), PixelFont.Hex("#e07a10") };
            Color32[] white = { PixelFont.Hex("#ffffff"), PixelFont.Hex("#ffffff"), PixelFont.Hex("#d8d4ff") };

            titleTex = PixelFont.Render("PIXEL\nADVENTURE", gold, PixelFont.Hex("#6a2a08"), 1);
            playTex = PixelFont.Render("PLAY", white);
            exitTex = PixelFont.Render("EXIT", white);

            Show(title, titleTex, titlePixelSize);
            Show(playLabel, playTex, labelPixelSize);
            Show(exitLabel, exitTex, labelPixelSize);
            titleBasePos = title.rectTransform.anchoredPosition;

            playButton.onClick.AddListener(Play);
            exitButton.onClick.AddListener(Exit);

            // Apple discourages quit buttons on iOS; the home gesture exits instead.
            if (Application.platform == RuntimePlatform.IPhonePlayer) exitButton.gameObject.SetActive(false);
        }

        void Update()
        {
            // Gentle bob, snapped to whole title pixels so it stays pixel-art.
            float bob = Mathf.Round(Mathf.Sin(Time.unscaledTime * 2f)) * titlePixelSize * 0.5f;
            title.rectTransform.anchoredPosition = titleBasePos + new Vector2(0f, bob);
        }

        static void Show(RawImage image, Texture2D tex, float pixelSize)
        {
            image.texture = tex;
            image.rectTransform.sizeDelta = new Vector2(tex.width, tex.height) * pixelSize;
        }

        void Play()
        {
            if (Application.CanStreamedLevelBeLoaded(gameScene)) SceneManager.LoadScene(gameScene);
            else Debug.LogWarning($"MainMenu: scene '{gameScene}' is not in Build Settings.");
        }

        void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnDestroy()
        {
            if (titleTex != null) Destroy(titleTex);
            if (playTex != null) Destroy(playTex);
            if (exitTex != null) Destroy(exitTex);
        }
    }
}
