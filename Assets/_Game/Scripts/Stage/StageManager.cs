using System.Collections;
using PixelAdventure.Player;
using PixelAdventure.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PixelAdventure.Stage
{
    /// <summary>
    /// Runs a stage: "STAGE 1-1" banner at start, clear banner at the goal, restart on death.
    /// Lives, checkpoints and waves arrive in Milestone 2.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [SerializeField] string stageName = "STAGE 1-1";
        [SerializeField] PlayerCombat player;
        [SerializeField] RawImage banner;
        [SerializeField] RawImage controlsHint;
        [SerializeField] float goalX = 165f;
        [SerializeField] string menuScene = "MainMenu";

        public const int PlayerLayer = 9;
        public const int EnemyLayer = 10;

        Texture2D bannerTex;
        Texture2D hintTex;
        bool finished;

        void Awake()
        {
            // Player and enemies pass through each other (Metal Slug style); only attacks connect.
            Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, true);
            Time.timeScale = 1f;
        }

        void Start()
        {
            player.Died += () => StartCoroutine(RestartAfterDeath());
            StartCoroutine(ShowBanner(stageName + "\nGO", 2.2f));

            if (controlsHint != null)
            {
                if (Application.isMobilePlatform) controlsHint.gameObject.SetActive(false);
                else
                {
                    hintTex = PixelFont.Render("AD MOVE  W JUMP  J ATTACK  K ROLL  L BASH  I ULTRA",
                        new[] { PixelFont.Hex("#d8d4ff") });
                    controlsHint.texture = hintTex;
                    controlsHint.rectTransform.sizeDelta = new Vector2(hintTex.width, hintTex.height) * 3f;
                }
            }
        }

        void Update()
        {
            if (!finished && player.transform.position.x >= goalX) StartCoroutine(Clear());
        }

        IEnumerator ShowBanner(string text, float hold)
        {
            if (bannerTex != null) Destroy(bannerTex);
            bannerTex = PixelFont.Render(text,
                new[] { PixelFont.Hex("#fff3a0"), PixelFont.Hex("#ffd24d"), PixelFont.Hex("#ffb000"), PixelFont.Hex("#e07a10") },
                PixelFont.Hex("#4a1a04"), 1);
            banner.texture = bannerTex;
            banner.rectTransform.sizeDelta = new Vector2(bannerTex.width, bannerTex.height) * 12f;
            banner.gameObject.SetActive(true);

            SetAlpha(1f);
            yield return new WaitForSecondsRealtime(hold);
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                SetAlpha(1f - t / 0.4f);
                yield return null;
            }
            banner.gameObject.SetActive(false);
        }

        void SetAlpha(float a) => banner.color = new Color(1f, 1f, 1f, a);

        IEnumerator Clear()
        {
            finished = true;
            yield return ShowBanner(stageName + "\nCLEAR", 2.5f);
            SceneManager.LoadScene(menuScene);
        }

        IEnumerator RestartAfterDeath()
        {
            finished = true;
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            if (bannerTex != null) Destroy(bannerTex);
            if (hintTex != null) Destroy(hintTex);
        }
    }
}
