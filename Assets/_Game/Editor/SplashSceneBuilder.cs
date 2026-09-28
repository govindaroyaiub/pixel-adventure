using PixelAdventure.Splash;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static PixelAdventure.EditorTools.SceneBuilderUtil;

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
        [MenuItem("Pixel Adventure/Build Splash Scene", priority = 20)]
        public static void Build()
        {
            if (!CanBuild()) return;
            CreateScene();
            ApplyBuildOrder();
            ApplyPlayerSettings();
            Debug.Log("Pixel Adventure: Splash scene built at " + SplashPath + ". Press Play to watch the intro.");
        }

        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(Color.black);
            var canvas = CreateCanvas();

            var rect = CreateUI("Logo", canvas.transform);
            Stretch(rect);
            var image = rect.gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 16f / 9f;

            var intro = new GameObject("SplashIntro").AddComponent<SplashIntro>();
            var so = new SerializedObject(intro);
            so.FindProperty("target").objectReferenceValue = image;
            so.ApplyModifiedPropertiesWithoutUndo();

            Save(scene, SplashPath);
        }

        public static void ApplyPlayerSettings()
        {
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
        }
    }
}
