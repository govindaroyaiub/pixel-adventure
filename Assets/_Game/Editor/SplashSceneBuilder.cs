using System.Collections.Generic;
using System.IO;
using System.Linq;
using PixelAdventure.Splash;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PixelAdventure.EditorTools
{
    /// <summary>
    /// Pixel Adventure → Build Splash Scene: generates the High Devs intro scene,
    /// puts it first in Build Settings and applies basic player settings
    /// (Unity splash on, black background, so it leads into the High Devs intro).
    /// Safe to run again; it overwrites the generated scene.
    /// </summary>
    public static class SplashSceneBuilder
    {
        const string SceneDir = "Assets/_Game/Scenes";
        const string ScenePath = SceneDir + "/Splash.unity";

        [MenuItem("Pixel Adventure/Build Splash Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Pixel Adventure",
                    "Stop Play mode first (click the ▶ button so it is no longer blue), then run Build Splash Scene again.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var logoGo = new GameObject("Logo", typeof(RectTransform));
            logoGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)logoGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = logoGo.AddComponent<RawImage>();
            image.raycastTarget = false;
            var fitter = logoGo.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 16f / 9f;

            var introGo = new GameObject("SplashIntro");
            var intro = introGo.AddComponent<SplashIntro>();
            var so = new SerializedObject(intro);
            so.FindProperty("target").objectReferenceValue = image;
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(SceneDir);
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            scenes.AddRange(EditorBuildSettings.scenes.Where(s => s.path != ScenePath));
            if (!scenes.Any(s => s.path == "Assets/Scenes/SampleScene.unity") && File.Exists("Assets/Scenes/SampleScene.unity"))
                scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true));
            EditorBuildSettings.scenes = scenes.ToArray();

            PlayerSettings.companyName = "High Devs";
            PlayerSettings.productName = "Pixel Adventure";
            // Unity's "Made with Unity" splash plays first, then our High Devs scene.
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = true;
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.UnityLogoBelow;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            AssetDatabase.SaveAssets();

            Debug.Log("Pixel Adventure: Splash scene built at " + ScenePath + ". Press Play to watch the intro.");
        }
    }
}
