using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PixelAdventure.EditorTools
{
    /// <summary>Shared helpers for the Pixel Adventure scene generators.</summary>
    public static class SceneBuilderUtil
    {
        public const string SceneDir = "Assets/_Game/Scenes";
        public const string SplashPath = SceneDir + "/Splash.unity";
        public const string MainMenuPath = SceneDir + "/MainMenu.unity";

        /// <summary>False (with a popup) if scenes can't be built right now.</summary>
        public static bool CanBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Pixel Adventure",
                    "Stop Play mode first (click the ▶ button so it is no longer blue), then run the menu item again.", "OK");
                return false;
            }
            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        public static void CreateCamera(Color background)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.orthographic = true;
            camGo.AddComponent<AudioListener>();
        }

        public static Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static EventSystem CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            var es = go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            return es;
        }

        public static RectTransform CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Save(UnityEngine.SceneManagement.Scene scene, string path)
        {
            Directory.CreateDirectory(SceneDir);
            EditorSceneManager.SaveScene(scene, path);
        }

        /// <summary>Build order: Splash, MainMenu, then every other scene already listed.</summary>
        public static void ApplyBuildOrder()
        {
            var ordered = new List<EditorBuildSettingsScene>();
            foreach (string path in new[] { SplashPath, MainMenuPath })
                if (File.Exists(path)) ordered.Add(new EditorBuildSettingsScene(path, true));
            ordered.AddRange(EditorBuildSettings.scenes.Where(s => ordered.All(o => o.path != s.path)));
            if (ordered.All(s => s.path != "Assets/Scenes/SampleScene.unity") && File.Exists("Assets/Scenes/SampleScene.unity"))
                ordered.Add(new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true));
            EditorBuildSettings.scenes = ordered.ToArray();
        }
    }
}
