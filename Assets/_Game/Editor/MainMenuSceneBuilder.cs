using PixelAdventure.Audio;
using PixelAdventure.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static PixelAdventure.EditorTools.SceneBuilderUtil;

namespace PixelAdventure.EditorTools
{
    /// <summary>
    /// Pixel Adventure → Build Main Menu Scene: title + Play / Exit buttons + looping menu music.
    /// Pixel Adventure → Build All Scenes: music + Splash + Main Menu + build order + player settings.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        static readonly Color Background = new Color32(10, 0, 20, 255);
        static readonly Color ButtonNormal = new Color32(58, 31, 110, 255);
        static readonly Color ButtonHover = new Color32(90, 50, 168, 255);
        static readonly Color ButtonPressed = new Color32(34, 16, 63, 255);
        static readonly Color ButtonBorder = new Color32(255, 176, 0, 255);

        [MenuItem("Pixel Adventure/Build All Scenes", priority = 0)]
        public static void BuildAll()
        {
            if (!CanBuild()) return;
            MusicGenerator.Generate();
            SplashSceneBuilder.CreateScene();
            CreateScene();
            ApplyBuildOrder();
            SplashSceneBuilder.ApplyPlayerSettings();
            EditorSceneManager.OpenScene(SplashPath);
            Debug.Log("Pixel Adventure: all scenes built. Splash is open — press Play.");
        }

        [MenuItem("Pixel Adventure/Build Main Menu Scene", priority = 21)]
        public static void Build()
        {
            if (!CanBuild()) return;
            CreateScene();
            ApplyBuildOrder();
            Debug.Log("Pixel Adventure: Main Menu scene built at " + MainMenuPath + ".");
        }

        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(Background);
            var canvas = CreateCanvas();
            var eventSystem = CreateEventSystem();

            var bg = CreateUI("Background", canvas.transform);
            Stretch(bg);
            bg.gameObject.AddComponent<Image>().color = Background;

            var title = CreateUI("Title", canvas.transform);
            title.anchoredPosition = new Vector2(0, 200);
            var titleImage = title.gameObject.AddComponent<RawImage>();
            titleImage.raycastTarget = false;

            var (playButton, playLabel) = CreateButton("PlayButton", canvas.transform, new Vector2(0, -110));
            var (exitButton, exitLabel) = CreateButton("ExitButton", canvas.transform, new Vector2(0, -280));

            var nav = playButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = exitButton;
            playButton.navigation = nav;
            nav.selectOnDown = null;
            nav.selectOnUp = playButton;
            exitButton.navigation = nav;
            eventSystem.firstSelectedGameObject = playButton.gameObject;

            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            var so = new SerializedObject(menu);
            so.FindProperty("title").objectReferenceValue = titleImage;
            so.FindProperty("playButton").objectReferenceValue = playButton;
            so.FindProperty("playLabel").objectReferenceValue = playLabel;
            so.FindProperty("exitButton").objectReferenceValue = exitButton;
            so.FindProperty("exitLabel").objectReferenceValue = exitLabel;
            so.ApplyModifiedPropertiesWithoutUndo();

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicGenerator.MenuThemePath);
            if (clip == null) clip = MusicGenerator.Generate();
            var music = new GameObject("Music").AddComponent<MusicPlayer>();
            var musicSo = new SerializedObject(music);
            musicSo.FindProperty("clip").objectReferenceValue = clip;
            musicSo.ApplyModifiedPropertiesWithoutUndo();

            Save(scene, MainMenuPath);
        }

        static (Button, RawImage) CreateButton(string name, Transform parent, Vector2 position)
        {
            var rect = CreateUI(name, parent);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(440, 130);

            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = ButtonBorder;
            outline.effectDistance = new Vector2(8, -8);

            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonHover;
            colors.selectedColor = ButtonHover;
            colors.pressedColor = ButtonPressed;
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            var label = CreateUI("Label", rect);
            var labelImage = label.gameObject.AddComponent<RawImage>();
            labelImage.raycastTarget = false;
            return (button, labelImage);
        }
    }
}
