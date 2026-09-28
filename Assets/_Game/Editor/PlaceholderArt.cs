using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PixelAdventure.EditorTools
{
    /// <summary>
    /// Generates the pixel-art placeholders (sky, mountains, ground tiles, dummy, UI shapes)
    /// as PNG sprites, plus pixel-art import settings for character sheets.
    /// </summary>
    public static class PlaceholderArt
    {
        const string Dir = "Assets/_Game/Art/Generated";

        public static Sprite Sky, MountainsFar, MountainsNear, Dirt, GrassTop, Stone, Dummy, Circle, Ring, White;

        public static void GenerateAll()
        {
            Directory.CreateDirectory(Dir);
            Sky = Save("Sky", Gradient(16, 90, "#140a2e", "#3b1f5e", "#8a3a78", "#d0687a", "#f0a070"), 9, Center, TextureWrapMode.Clamp);
            MountainsFar = Save("MountainsFar", Mountains(128, 48, 20, new[] { (2, 10f, 0f), (5, 6f, 1f), (11, 3f, 2f) }, "#4a2d6b", "#6b4a8c"), 8, Center, TextureWrapMode.Repeat);
            MountainsNear = Save("MountainsNear", Mountains(128, 40, 14, new[] { (3, 8f, 2f), (7, 4f, 0.5f) }, "#2a1845", "#3d2560"), 8, Center, TextureWrapMode.Repeat);
            Dirt = Save("Dirt", Tile(false), 16, Center, TextureWrapMode.Repeat);
            GrassTop = Save("GrassTop", Tile(true), 16, Center, TextureWrapMode.Repeat);
            Stone = Save("Stone", StoneTile(), 16, Center, TextureWrapMode.Repeat);
            Dummy = Save("Dummy", DummyTex(), 16, new Vector2(0.5f, 0f), TextureWrapMode.Clamp);
            Circle = Save("Circle", Disc(64, 0), 100, Center, TextureWrapMode.Clamp);
            Ring = Save("Ring", Disc(64, 5), 32, Center, TextureWrapMode.Clamp);
            White = Save("White", Fill(4, 4, Color.white), 4, Center, TextureWrapMode.Clamp);
        }

        /// <summary>Pixel-art import settings for a character sprite sheet (sliced at runtime by SpriteAnimator).</summary>
        public static Texture2D ConfigureSheet(string path)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null) return null;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        static Sprite Save(string name, Texture2D tex, float ppu, Vector2 pivot, TextureWrapMode wrap)
        {
            string path = $"{Dir}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.wrapMode = wrap;
            imp.spritePixelsPerUnit = ppu;
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // required for tiled drawing
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            imp.SetTextureSettings(settings);
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------- Texture painters (y = 0 is the TOP row in all helpers) ----------

        static Texture2D New(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(new Color32[w * h]);
            return tex;
        }

        static void Set(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) return;
            tex.SetPixel(x, tex.height - 1 - y, c);
        }

        static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        static Texture2D Fill(int w, int h, Color c)
        {
            var tex = New(w, h);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) Set(tex, x, y, c);
            tex.Apply();
            return tex;
        }

        static Texture2D Gradient(int w, int h, params string[] stops)
        {
            var tex = New(w, h);
            for (int y = 0; y < h; y++)
            {
                // Banded (not smooth) for a pixel-art sky.
                float t = (float)y / (h - 1) * (stops.Length - 1);
                int i = Mathf.Min(stops.Length - 2, Mathf.FloorToInt(t));
                float f = Mathf.Floor((t - i) * 6f) / 6f;
                Color c = Color.Lerp(C(stops[i]), C(stops[i + 1]), f);
                for (int x = 0; x < w; x++) Set(tex, x, y, c);
            }
            tex.Apply();
            return tex;
        }

        static Texture2D Mountains(int w, int h, int baseHeight, (int freq, float amp, float phase)[] waves, string body, string ridge)
        {
            var tex = New(w, h);
            for (int x = 0; x < w; x++)
            {
                float height = baseHeight;
                foreach (var (freq, amp, phase) in waves) height += amp * Mathf.Sin(x * Mathf.PI * 2f / w * freq + phase);
                int top = h - Mathf.Clamp(Mathf.RoundToInt(height), 1, h);
                for (int y = top; y < h; y++) Set(tex, x, y, y < top + 2 ? C(ridge) : C(body));
            }
            tex.Apply();
            return tex;
        }

        static float Hash(int x, int y) => Mathf.Abs(Mathf.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f) % 1f;

        static Texture2D Tile(bool grassTop)
        {
            var tex = New(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float n = Hash(x, y);
                    Color c = n < 0.12f ? C("#4a2c18") : n > 0.9f ? C("#8a5a36") : C("#6b4226");
                    if (grassTop)
                    {
                        int grassDepth = 4 + (Hash(x, 99) > 0.5f ? 1 : 0);
                        if (y == 0) c = C("#8fe05f");
                        else if (y < grassDepth - 1) c = C("#5fbf3f");
                        else if (y < grassDepth) c = C("#3a8a2a");
                    }
                    Set(tex, x, y, c);
                }
            tex.Apply();
            return tex;
        }

        static Texture2D StoneTile()
        {
            var tex = New(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool mortarRow = y % 8 == 7;
                    bool mortarCol = (y < 8 ? x : (x + 8) % 16) % 16 == 15;
                    Color c = mortarRow || mortarCol ? C("#4a3a5a") : Hash(x, y) > 0.85f ? C("#8a7a9a") : C("#7a6a8a");
                    if (y == 0) c = C("#a898b8");
                    Set(tex, x, y, c);
                }
            tex.Apply();
            return tex;
        }

        static Texture2D DummyTex()
        {
            var tex = New(16, 24);
            Color post = C("#6b4226"), straw = C("#d8b048"), strawDark = C("#a88028"), sack = C("#e8d8b0"), sackDark = C("#b8a880"), eye = C("#3a2020");
            for (int y = 14; y < 24; y++) { Set(tex, 7, y, post); Set(tex, 8, y, post); }
            for (int x = 0; x < 16; x++) { Set(tex, x, 9, post); Set(tex, x, 10, strawDark); }
            for (int y = 6; y < 16; y++)
                for (int x = 3; x < 13; x++)
                    Set(tex, x, y, y == 8 || y == 13 ? C("#8a5a36") : (x + y) % 3 == 0 ? strawDark : straw);
            for (int y = 0; y < 7; y++)
                for (int x = 4; x < 12; x++)
                {
                    bool corner = (y == 0 || y == 6) && (x == 4 || x == 11);
                    if (!corner) Set(tex, x, y, x == 11 || y == 6 ? sackDark : sack);
                }
            Set(tex, 6, 2, eye); Set(tex, 5, 3, eye); Set(tex, 7, 3, eye); Set(tex, 6, 4, eye);
            Set(tex, 9, 2, eye); Set(tex, 10, 3, eye); Set(tex, 8, 3, eye); Set(tex, 9, 4, eye);
            tex.Apply();
            return tex;
        }

        static Texture2D Disc(int size, int ringThickness)
        {
            var tex = New(size, size);
            float r = size / 2f - 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - r) * (x - r) + (y - r) * (y - r));
                    bool on = d <= r && (ringThickness == 0 || d >= r - ringThickness);
                    if (on) Set(tex, x, y, Color.white);
                }
            tex.Apply();
            return tex;
        }
    }
}
