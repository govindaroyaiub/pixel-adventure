using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.Splash
{
    public struct LogoPixel
    {
        public int X;
        public int Y; // top-down, 0 = top row
        public Color32 Color;
        public float Delay;
    }

    /// <summary>
    /// Builds the "High Devs" arcade logo as a 160x90 pixel list.
    /// C# port of Branding/high-devs-logo-concepts.html (concept 3).
    /// </summary>
    public static class HighDevsLogo
    {
        public const int Width = 160;
        public const int Height = 90;

        public static readonly Color32 Background = Hex("#0a0014");

        static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
        {
            ['H'] = new[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" },
            ['I'] = new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" },
            ['G'] = new[] { "01110", "10001", "10000", "10111", "10001", "10001", "01111" },
            ['D'] = new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" },
            ['E'] = new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" },
            ['V'] = new[] { "10001", "10001", "10001", "10001", "10001", "01010", "00100" },
            ['S'] = new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" },
        };

        public static List<LogoPixel> Build(int seed = 1234)
        {
            var grid = new Color32?[Width, Height];

            void Put(float x, float y, Color32 c)
            {
                int ix = Mathf.RoundToInt(x), iy = Mathf.RoundToInt(y);
                if (ix >= 0 && ix < Width && iy >= 0 && iy < Height) grid[ix, iy] = c;
            }

            // Synthwave grid floor
            const int horizon = 70;
            Color32 gridFar = Hex("#2a0845"), gridNear = Hex("#b0209a");
            foreach (int off in new[] { 0, 2, 5, 9, 14, 19 })
            {
                int y = horizon + off;
                float k = (float)off / (Height - horizon);
                for (int x = 0; x < Width; x++) Put(x, y, Color32.Lerp(gridFar, gridNear, k));
            }
            for (int i = -10; i <= 10; i++)
            {
                for (int s = 0; s <= 400; s++)
                {
                    float t = s / 400f;
                    Put(80 + i * 5 + i * 20 * t, horizon + t * (Height - horizon), Color32.Lerp(gridFar, gridNear, t));
                }
            }

            // 3D wordmark
            const int sc = 3, tx = 36;
            int tw = TextWidth("HIGH", sc);

            void Layers(string str, int y, Color32 extrude, Color32[] front)
            {
                for (int d = 3; d >= 1; d--) DrawText(Put, str, tx + d, y + d, sc, _ => extrude);
                DrawText(Put, str, tx, y, sc, row => Bands(front, row, 7 * sc));
            }

            Layers("HIGH", 12, Hex("#0b4f6a"), new[] { Hex("#d8ffff"), Hex("#6ff6ff"), Hex("#28d4f0"), Hex("#1aa6d8") });
            Layers("DEVS", 40, Hex("#6a0b58"), new[] { Hex("#ffe0fb"), Hex("#ff7ae6"), Hex("#ff3fd0"), Hex("#c21ea6") });

            // Double up-arrow
            void Arrow(int ax, int ay, Color32 top, Color32 bottom)
            {
                for (int k = 0; k < 7; k++)
                    for (int x = ax - k; x <= ax + k; x++)
                        Put(x, ay + k, k < 3 ? top : bottom);
            }

            Arrow(tx + tw + 14, 14, Hex("#fff3a0"), Hex("#ffe14d"));
            Arrow(tx + tw + 14, 24, Hex("#ffd24d"), Hex("#ffb000"));

            // Stars
            Color32 white = Hex("#ffffff");
            int[,] stars = { { 12, 8 }, { 148, 12 }, { 140, 40 }, { 20, 44 }, { 128, 60 }, { 8, 62 } };
            for (int i = 0; i < stars.GetLength(0); i++) Put(stars[i, 0], stars[i, 1], white);

            // Flatten with reveal delays (sweeps left to right, randomized)
            var rng = new System.Random(seed);
            var pixels = new List<LogoPixel>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (grid[x, y].HasValue)
                        pixels.Add(new LogoPixel
                        {
                            X = x,
                            Y = y,
                            Color = grid[x, y].Value,
                            Delay = (float)rng.NextDouble() * 0.7f + (float)x / Width * 0.5f,
                        });
            return pixels;
        }

        public static int TextWidth(string str, int scale)
        {
            int w = 0;
            foreach (char ch in str) w += ch == ' ' ? 4 * scale : 6 * scale;
            return w - scale;
        }

        static void DrawText(Action<float, float, Color32> put, string str, int ox, int oy, int scale, Func<int, Color32> color)
        {
            int cx = ox;
            foreach (char ch in str)
            {
                if (ch == ' ') { cx += 4 * scale; continue; }
                string[] g = Font[ch];
                for (int r = 0; r < 7; r++)
                    for (int c = 0; c < 5; c++)
                        if (g[r][c] == '1')
                            for (int dy = 0; dy < scale; dy++)
                                for (int dx = 0; dx < scale; dx++)
                                    put(cx + c * scale + dx, oy + r * scale + dy, color(r * scale + dy));
                cx += 6 * scale;
            }
        }

        static Color32 Bands(Color32[] stops, int row, int rows)
        {
            return stops[Mathf.Min(stops.Length - 1, Mathf.FloorToInt((float)row / rows * stops.Length))];
        }

        static Color32 Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
