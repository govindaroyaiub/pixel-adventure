using System.IO;
using PixelAdventure.Anim;
using PixelAdventure.Combat;
using PixelAdventure.Controls;
using PixelAdventure.Player;
using PixelAdventure.Stage;
using PixelAdventure.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static PixelAdventure.EditorTools.SceneBuilderUtil;

namespace PixelAdventure.EditorTools
{
    /// <summary>
    /// Pixel Adventure → Build Stage 1-1: side-scrolling level, playable Knight, training dummies,
    /// touch HUD (joystick + ATK / ROLL / BASH / ULT). Re-running rebuilds the scene but keeps
    /// the Knight data asset, so tuning in the Inspector is never lost.
    /// </summary>
    public static class Stage1SceneBuilder
    {
        const string KnightArt = "Assets/_Game/Art/Characters/Knight";
        const string AnimSetPath = "Assets/_Game/Data/Animations/KnightAnimations.asset";
        const string KnightDataPath = "Assets/_Game/Data/HeroClasses/Knight.asset";
        const string NoFrictionPath = "Assets/_Game/Data/Physics/NoFriction.physicsMaterial2D";

        const int GroundLayer = 8;
        const float LevelMinX = -15f;
        const float LevelMaxX = 172f;

        static readonly Color SkyColor = new Color32(20, 10, 46, 255);

        // (center x, top y, width)
        static readonly (float x, float top, float w)[] Platforms =
        {
            (16, 2.2f, 4), (23, 3.6f, 3), (38, 2.2f, 5), (52, 2.8f, 3), (58, 4.2f, 4),
            (75, 2.2f, 6), (92, 3f, 4), (110, 2.2f, 5), (128, 3.4f, 4), (145, 2.4f, 6),
        };

        static readonly float[] DummyXs = { -2, 1, 4, 30, 45, 66, 85, 100, 120, 138, 155 };

        [MenuItem("Pixel Adventure/Build Stage 1-1", priority = 23)]
        public static void Build()
        {
            if (!CanBuild()) return;
            PlaceholderArt.GenerateAll();
            CreateScene();
            ApplyBuildOrder();
            EditorSceneManager.OpenScene(Stage1Path);
            Debug.Log("Pixel Adventure: Stage 1-1 built at " + Stage1Path + ". Press Play.");
        }

        public static void CreateScene()
        {
            // NewScene unloads unreferenced assets, so load everything AFTER creating it.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureLayers();
            HeroClassData knight = EnsureKnightAssets();
            var noFriction = EnsureNoFriction();
            PlaceholderArt.Load();

            // Camera + backdrop
            CreateCamera(SkyColor);
            var cam = Camera.main;
            cam.transform.position = new Vector3(0, 3, -10);
            cam.orthographicSize = 5f;
            var stageCam = cam.gameObject.AddComponent<StageCamera>();

            var sky = Sprite2D("Sky", PlaceholderArt.Sky, -100, cam.transform);
            sky.transform.localPosition = new Vector3(0, 0, 10);
            sky.transform.localScale = new Vector3(12f, 1.15f, 1f);
            BackgroundLayer("MountainsFar", PlaceholderArt.MountainsFar, new Vector2(400, 6), 2.6f, -90, 0.8f, cam.transform);
            BackgroundLayer("MountainsNear", PlaceholderArt.MountainsNear, new Vector2(400, 5), 2f, -80, 0.5f, cam.transform);

            // Level geometry
            var level = new GameObject("Level").transform;
            float groundWidth = LevelMaxX - LevelMinX + 6f;
            var ground = Block("Ground", level, new Vector2((LevelMinX + LevelMaxX) / 2f, -3f), new Vector2(groundWidth, 6f), PlaceholderArt.Dirt, 0);
            var grass = Sprite2D("Grass", PlaceholderArt.GrassTop, 1, ground.transform);
            grass.drawMode = SpriteDrawMode.Tiled;
            grass.size = new Vector2(groundWidth, 1f);
            grass.transform.localPosition = new Vector3(0, 2.5f, 0);

            foreach (var (x, top, w) in Platforms)
            {
                var p = Block("Platform", level, new Vector2(x, top - 0.375f), new Vector2(w, 0.75f), PlaceholderArt.Stone, 2);
                p.GetComponent<BoxCollider2D>().usedByEffector = true;
                var effector = p.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 170f;
            }
            Wall("WallLeft", level, LevelMinX - 0.5f);
            Wall("WallRight", level, LevelMaxX + 0.5f);

            // HUD first (the input reader needs its controls)
            var canvas = CreateCanvas();
            CreateEventSystem();
            var (joystick, attack, roll, primary, ultra) = CreateControls(canvas.transform);
            var inputReader = new GameObject("Input").AddComponent<PlayerInputReader>();
            SetRefs(inputReader, ("joystick", joystick), ("attackButton", attack), ("rollButton", roll),
                ("primaryButton", primary), ("ultraButton", ultra));

            // Player
            var (motor, combat, health) = Knight(knight, inputReader, noFriction);
            SetRefs(stageCam, ("target", motor));
            SetFloat(stageCam, "levelMinX", LevelMinX);
            SetFloat(stageCam, "levelMaxX", LevelMaxX);

            HudHealth(canvas.transform, health);

            // Training dummies
            var dummies = new GameObject("Dummies").transform;
            foreach (float x in DummyXs) CreateDummy(dummies, x);

            // Stage flow
            var banner = UI<RawImage>("Banner", canvas.transform, Vector2.zero, new Vector2(0.5f, 0.6f));
            banner.raycastTarget = false;
            var hint = UI<RawImage>("ControlsHint", canvas.transform, new Vector2(0, -40), new Vector2(0.5f, 1f));
            hint.rectTransform.pivot = new Vector2(0.5f, 1f);
            hint.raycastTarget = false;
            var manager = new GameObject("StageManager").AddComponent<StageManager>();
            SetRefs(manager, ("player", combat), ("banner", banner), ("controlsHint", hint));
            SetFloat(manager, "goalX", LevelMaxX - 7f);

            Save(scene, Stage1Path);
        }

        // ---------- Player ----------

        static (PlayerMotor, PlayerCombat, Health) Knight(HeroClassData data, PlayerInputReader input, PhysicsMaterial2D material)
        {
            var go = new GameObject("Knight") { layer = StageManager.PlayerLayer };
            go.transform.position = new Vector3(-8f, 0.05f, 0);
            go.AddComponent<Rigidbody2D>();
            var box = go.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(0, 0.7f);
            box.size = new Vector2(0.6f, 1.4f);
            box.sharedMaterial = material;
            var health = go.AddComponent<Health>();
            var hurtbox = go.AddComponent<Hurtbox>();
            SetEnum(hurtbox, "team", (int)Team.Player);
            var motor = go.AddComponent<PlayerMotor>();
            SetRefs(motor, ("data", data), ("input", input));
            var so = new SerializedObject(motor);
            so.FindProperty("groundMask").intValue = 1 << GroundLayer;
            so.ApplyModifiedPropertiesWithoutUndo();

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            spriteGo.transform.localPosition = new Vector3(0, 0.77f, 0); // body center, so the roll spins nicely
            var sr = spriteGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            var animator = spriteGo.AddComponent<SpriteAnimator>();
            SetRefs(animator, ("set", data.animations));

            var combat = go.AddComponent<PlayerCombat>();
            SetRefs(combat, ("animator", animator), ("slamRing", PlaceholderArt.Ring));
            return (motor, combat, health);
        }

        static HeroClassData EnsureKnightAssets()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AnimSetPath));
            Directory.CreateDirectory(Path.GetDirectoryName(KnightDataPath));

            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(AnimSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<SpriteAnimationSet>();
                AssetDatabase.CreateAsset(set, AnimSetPath);
            }
            set.frameWidth = 96;
            set.frameHeight = 84;
            set.pivot = new Vector2(0.49f, 0.488f);
            set.pixelsPerUnit = 24f;
            set.clips.Clear();
            (string name, string file, float fps, bool loop)[] clips =
            {
                ("Idle", "IDLE", 8, true), ("Run", "RUN", 12, true), ("Jump", "JUMP", 10, false),
                ("Attack1", "ATTACK 1", 16, false), ("Attack2", "ATTACK 2", 16, false), ("Attack3", "ATTACK 3", 16, false),
                ("Defend", "DEFEND", 12, false), ("Hurt", "HURT", 14, false), ("Death", "DEATH", 10, false),
            };
            foreach (var (name, file, fps, loop) in clips)
            {
                var tex = PlaceholderArt.ConfigureSheet($"{KnightArt}/{file}.png");
                set.clips.Add(new SpriteAnimationSet.Clip { name = name, texture = tex, fps = fps, loop = loop });
            }
            EditorUtility.SetDirty(set);

            var data = AssetDatabase.LoadAssetAtPath<HeroClassData>(KnightDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<HeroClassData>();
                AssetDatabase.CreateAsset(data, KnightDataPath);
            }
            data.animations = set;
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            return data;
        }

        static PhysicsMaterial2D EnsureNoFriction()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(NoFrictionPath);
            if (mat != null) return mat;
            Directory.CreateDirectory(Path.GetDirectoryName(NoFrictionPath));
            mat = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, NoFrictionPath);
            return mat;
        }

        // ---------- Level pieces ----------

        static GameObject Block(string name, Transform parent, Vector2 center, Vector2 size, Sprite sprite, int order)
        {
            var go = new GameObject(name) { layer = GroundLayer };
            go.transform.SetParent(parent);
            go.transform.position = center;
            go.AddComponent<BoxCollider2D>().size = size;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = order;
            return go;
        }

        static void Wall(string name, Transform parent, float x)
        {
            var go = new GameObject(name) { layer = GroundLayer };
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, 5f, 0);
            go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 20f);
        }

        static SpriteRenderer Sprite2D(string name, Sprite sprite, int order, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static void BackgroundLayer(string name, Sprite sprite, Vector2 size, float y, int order, float follow, Transform cam)
        {
            var sr = Sprite2D(name, sprite, order);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.transform.position = new Vector3(0, y, 0);
            var parallax = sr.gameObject.AddComponent<Parallax>();
            SetRefs(parallax, ("cam", cam));
            SetFloat(parallax, "follow", follow);
        }

        static void CreateDummy(Transform parent, float x)
        {
            var go = new GameObject("Dummy") { layer = StageManager.EnemyLayer };
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, 0.05f, 0);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.linearDamping = 3f;
            var box = go.AddComponent<BoxCollider2D>();
            box.offset = new Vector2(0, 0.75f);
            box.size = new Vector2(0.8f, 1.5f);
            var health = go.AddComponent<Health>();
            SetInt(health, "max", 60);
            SetEnum(go.AddComponent<Hurtbox>(), "team", (int)Team.Enemy);

            var body = Sprite2D("Body", PlaceholderArt.Dummy, 5, go.transform);

            var bg = Sprite2D("HpBg", PlaceholderArt.White, 6, go.transform);
            bg.color = new Color32(26, 15, 46, 255);
            bg.transform.localPosition = new Vector3(0, 1.85f, 0);
            bg.transform.localScale = new Vector3(1f, 0.12f, 1f);
            var fillPivot = new GameObject("HpFill").transform;
            fillPivot.SetParent(go.transform, false);
            fillPivot.localPosition = new Vector3(-0.5f, 1.85f, 0);
            var fill = Sprite2D("Fill", PlaceholderArt.White, 7, fillPivot);
            fill.color = new Color32(232, 69, 90, 255);
            fill.transform.localPosition = new Vector3(0.5f, 0, 0);
            fill.transform.localScale = new Vector3(1f, 0.12f, 1f);

            SetRefs(go.AddComponent<TrainingDummy>(), ("body", body), ("hpFill", fillPivot));
        }

        // ---------- HUD ----------

        static (VirtualJoystick, TouchButton, TouchButton, TouchButton, TouchButton) CreateControls(Transform canvas)
        {
            var baseImg = UI<Image>("Joystick", canvas, new Vector2(280, 260), Vector2.zero);
            baseImg.sprite = PlaceholderArt.Circle;
            baseImg.color = new Color(1, 1, 1, 0.18f);
            baseImg.rectTransform.sizeDelta = new Vector2(300, 300);
            var knob = UI<Image>("Knob", baseImg.transform, Vector2.zero, new Vector2(0.5f, 0.5f));
            knob.sprite = PlaceholderArt.Circle;
            knob.color = new Color(1, 1, 1, 0.45f);
            knob.rectTransform.sizeDelta = new Vector2(140, 140);
            knob.raycastTarget = false;
            var joystick = baseImg.gameObject.AddComponent<VirtualJoystick>();
            SetRefs(joystick, ("knob", knob.rectTransform));

            var attack = ActionButton(canvas, "AttackButton", "ATK", new Vector2(-240, 210), 230, new Color(0.91f, 0.27f, 0.35f, 0.75f));
            var roll = ActionButton(canvas, "RollButton", "ROLL", new Vector2(-520, 150), 160, new Color(0.25f, 0.55f, 0.95f, 0.75f));
            var primary = ActionButton(canvas, "PrimaryButton", "BASH", new Vector2(-480, 390), 170, new Color(1f, 0.72f, 0.1f, 0.8f));
            var ultra = ActionButton(canvas, "UltraButton", "ULT", new Vector2(-250, 480), 180, new Color(0.62f, 0.3f, 0.95f, 0.8f));
            return (joystick, attack, roll, primary, ultra);
        }

        static TouchButton ActionButton(Transform canvas, string name, string label, Vector2 pos, float size, Color color)
        {
            var img = UI<Image>(name, canvas, pos, new Vector2(1, 0));
            img.sprite = PlaceholderArt.Circle;
            img.color = color;
            img.rectTransform.sizeDelta = new Vector2(size, size);

            var cooldown = UI<Image>("Cooldown", img.transform, Vector2.zero, new Vector2(0.5f, 0.5f));
            cooldown.sprite = PlaceholderArt.Circle;
            cooldown.color = new Color(0, 0, 0, 0.55f);
            cooldown.rectTransform.sizeDelta = new Vector2(size, size);
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Radial360;
            cooldown.fillOrigin = (int)Image.Origin360.Top;
            cooldown.fillClockwise = false;
            cooldown.fillAmount = 0f;
            cooldown.raycastTarget = false;

            var labelImg = UI<RawImage>("Label", img.transform, Vector2.zero, new Vector2(0.5f, 0.5f));
            labelImg.raycastTarget = false;

            var button = img.gameObject.AddComponent<TouchButton>();
            SetRefs(button, ("labelImage", labelImg), ("cooldownFill", cooldown));
            var so = new SerializedObject(button);
            so.FindProperty("label").stringValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
            return button;
        }

        static void HudHealth(Transform canvas, Health health)
        {
            var root = CreateUI("HealthBar", canvas);
            root.anchorMin = root.anchorMax = new Vector2(0, 1);
            root.pivot = new Vector2(0, 1);
            root.anchoredPosition = new Vector2(40, -40);

            var label = UI<RawImage>("Label", root, Vector2.zero, new Vector2(0, 1));
            label.rectTransform.pivot = new Vector2(0, 1);
            label.raycastTarget = false;

            var bg = UI<Image>("Background", root, new Vector2(90, -2), new Vector2(0, 1));
            bg.rectTransform.pivot = new Vector2(0, 1);
            bg.rectTransform.sizeDelta = new Vector2(420, 40);
            bg.sprite = PlaceholderArt.White;
            bg.color = new Color32(26, 15, 46, 230);
            bg.raycastTarget = false;
            var outline = bg.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(255, 176, 0, 255);
            outline.effectDistance = new Vector2(4, -4);

            Image Bar(string name, Color color)
            {
                var img = UI<Image>(name, bg.transform, Vector2.zero, Vector2.zero);
                Stretch(img.rectTransform);
                img.rectTransform.offsetMin = new Vector2(4, 4);
                img.rectTransform.offsetMax = new Vector2(-4, -4);
                img.sprite = PlaceholderArt.White;
                img.color = color;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.raycastTarget = false;
                return img;
            }

            var trail = Bar("Trail", new Color32(255, 224, 160, 255));
            var fill = Bar("Fill", new Color32(232, 69, 90, 255));
            var bar = root.gameObject.AddComponent<HealthBar>();
            SetRefs(bar, ("health", health), ("fill", fill), ("trail", trail), ("label", label));
        }

        static T UI<T>(string name, Transform parent, Vector2 pos, Vector2 anchor) where T : Graphic
        {
            var rect = CreateUI(name, parent);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = pos;
            return rect.gameObject.AddComponent<T>();
        }

        // ---------- Serialized field helpers ----------

        static void SetRefs(Object target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs) so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetEnum(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            (int index, string name)[] wanted = { (GroundLayer, "Ground"), (StageManager.PlayerLayer, "Player"), (StageManager.EnemyLayer, "Enemy") };
            foreach (var (index, name) in wanted)
            {
                var layer = layers.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(layer.stringValue)) layer.stringValue = name;
            }
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
