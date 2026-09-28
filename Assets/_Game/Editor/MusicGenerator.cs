using System.IO;
using PixelAdventure.Audio;
using UnityEditor;
using UnityEngine;

namespace PixelAdventure.EditorTools
{
    /// <summary>Pixel Adventure → Generate Menu Music: renders MenuTheme to a looping WAV asset.</summary>
    public static class MusicGenerator
    {
        public const string MenuThemePath = "Assets/_Game/Audio/MenuTheme.wav";
        const int SampleRate = 32000;

        [MenuItem("Pixel Adventure/Generate Menu Music", priority = 22)]
        public static void GenerateMenu()
        {
            Generate();
            Debug.Log("Pixel Adventure: menu music written to " + MenuThemePath + ". Select it in the Project window and press ▶ in the Inspector to listen.");
        }

        public static AudioClip Generate()
        {
            float[] samples = ChiptuneSynth.Render(MenuTheme.Build(), SampleRate);
            Directory.CreateDirectory(Path.GetDirectoryName(MenuThemePath));
            WriteWav(MenuThemePath, samples, SampleRate);
            AssetDatabase.ImportAsset(MenuThemePath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(MenuThemePath);
        }

        static void WriteWav(string path, float[] samples, int sampleRate)
        {
            using var writer = new BinaryWriter(File.Create(path));
            int dataBytes = samples.Length * 2;
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);                 // fmt chunk size
            writer.Write((short)1);           // PCM
            writer.Write((short)1);           // mono
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);     // byte rate
            writer.Write((short)2);           // block align
            writer.Write((short)16);          // bits per sample
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);
            foreach (float s in samples) writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(s * short.MaxValue), short.MinValue, short.MaxValue));
        }
    }
}
