using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.UI
{
    /// <summary>
    /// 5x7 bitmap font (A-Z, 0-9, space) used for the logo, title and button labels.
    /// </summary>
    public static class PixelFont
    {
        public const int GlyphHeight = 7;

        static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            ['A'] = new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" },
            ['B'] = new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" },
            ['C'] = new[] { "01110", "10001", "10000", "10000", "10000", "10001", "01110" },
            ['D'] = new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" },
            ['E'] = new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" },
            ['F'] = new[] { "11111", "10000", "10000", "11110", "10000", "10000", "10000" },
            ['G'] = new[] { "01110", "10001", "10000", "10111", "10001", "10001", "01111" },
            ['H'] = new[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" },
            ['I'] = new[] { "11111", "00100", "00100", "00100", "00100", "00100", "11111" },
            ['J'] = new[] { "00111", "00010", "00010", "00010", "00010", "10010", "01100" },
            ['K'] = new[] { "10001", "10010", "10100", "11000", "10100", "10010", "10001" },
            ['L'] = new[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" },
            ['M'] = new[] { "10001", "11011", "10101", "10101", "10001", "10001", "10001" },
            ['N'] = new[] { "10001", "10001", "11001", "10101", "10011", "10001", "10001" },
            ['O'] = new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" },
            ['P'] = new[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" },
            ['Q'] = new[] { "01110", "10001", "10001", "10001", "10101", "10010", "01101" },
            ['R'] = new[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" },
            ['S'] = new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" },
            ['T'] = new[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" },
            ['U'] = new[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" },
            ['V'] = new[] { "10001", "10001", "10001", "10001", "10001", "01010", "00100" },
            ['W'] = new[] { "10001", "10001", "10001", "10101", "10101", "10101", "01010" },
            ['X'] = new[] { "10001", "10001", "01010", "00100", "01010", "10001", "10001" },
            ['Y'] = new[] { "10001", "10001", "01010", "00100", "00100", "00100", "00100" },
            ['Z'] = new[] { "11111", "00001", "00010", "00100", "01000", "10000", "11111" },
            ['0'] = new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" },
            ['1'] = new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" },
            ['2'] = new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" },
            ['3'] = new[] { "11110", "00001", "00001", "01110", "00001", "00001", "11110" },
            ['4'] = new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" },
            ['5'] = new[] { "11111", "10000", "11110", "00001", "00001", "10001", "01110" },
            ['6'] = new[] { "00110", "01000", "10000", "11110", "10001", "10001", "01110" },
            ['7'] = new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" },
            ['8'] = new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" },
            ['9'] = new[] { "01110", "10001", "10001", "01111", "00001", "00010", "01100" },
            ['-'] = new[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" },
            ['!'] = new[] { "00100", "00100", "00100", "00100", "00100", "00000", "00100" },
            ['.'] = new[] { "00000", "00000", "00000", "00000", "00000", "00000", "00100" },
        };

        public static int TextWidth(string str, int scale)
        {
            int w = 0;
            foreach (char ch in str) w += ch == ' ' ? 4 * scale : 6 * scale;
            return w - scale;
        }

        /// <summary>Calls put(x, y, color) for every lit pixel; y is top-down. color(row) picks the color per pixel row.</summary>
        public static void Draw(Action<int, int, Color32> put, string str, int ox, int oy, int scale, Func<int, Color32> color)
        {
            int cx = ox;
            foreach (char raw in str)
            {
                char ch = char.ToUpperInvariant(raw);
                if (ch == ' ' || !Glyphs.TryGetValue(ch, out string[] g)) { cx += (ch == ' ' ? 4 : 6) * scale; continue; }
                for (int r = 0; r < GlyphHeight; r++)
                    for (int c = 0; c < 5; c++)
                        if (g[r][c] == '1')
                            for (int dy = 0; dy < scale; dy++)
                                for (int dx = 0; dx < scale; dx++)
                                    put(cx + c * scale + dx, oy + r * scale + dy, color(r * scale + dy));
                cx += 6 * scale;
            }
        }

        /// <summary>
        /// Renders centered lines ('\n' separated) to a transparent, point-filtered texture.
        /// Each line gets a vertical color gradient (bands) and an optional 3D extrusion.
        /// </summary>
        public static Texture2D Render(string text, Color32[] bands, Color32 extrude = default, int depth = 0)
        {
            string[] lines = text.Split('\n');
            int lineGap = 2;
            int width = 0;
            foreach (string line in lines) width = Mathf.Max(width, TextWidth(line, 1));
            width += depth;
            int height = lines.Length * GlyphHeight + (lines.Length - 1) * lineGap + depth;

            var pixels = new Color32[width * height];
            void Put(int x, int y, Color32 c)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                pixels[(height - 1 - y) * width + x] = c; // texture origin is bottom-left
            }

            for (int i = 0; i < lines.Length; i++)
            {
                int ox = (width - depth - TextWidth(lines[i], 1)) / 2;
                int oy = i * (GlyphHeight + lineGap);
                for (int d = depth; d >= 1; d--) Draw(Put, lines[i], ox + d, oy + d, 1, _ => extrude);
                Draw(Put, lines[i], ox, oy, 1, row => Bands(bands, row, GlyphHeight));
            }

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            tex.SetPixels32(pixels);
            tex.Apply(false);
            return tex;
        }

        public static Color32 Bands(Color32[] stops, int row, int rows)
        {
            return stops[Mathf.Min(stops.Length - 1, Mathf.FloorToInt((float)row / rows * stops.Length))];
        }

        public static Color32 Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
