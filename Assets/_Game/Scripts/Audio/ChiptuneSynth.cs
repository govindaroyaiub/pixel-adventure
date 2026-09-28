using System;
using System.Collections.Generic;

namespace PixelAdventure.Audio
{
    public enum Wave { Square25, Square12, Triangle }
    public enum Drum { Kick, Snare, Hat }

    public struct SynthNote
    {
        public float StartBeat;
        public float LengthBeats;
        public int Midi;
    }

    public class SynthTrack
    {
        public Wave Wave;
        public float Volume;
        public bool Pluck;   // fast decay (arpeggios)
        public bool Vibrato; // lead voice
        public readonly List<SynthNote> Notes = new List<SynthNote>();
    }

    public class Song
    {
        public float Bpm;
        public int Beats;
        public readonly List<SynthTrack> Tracks = new List<SynthTrack>();
        public readonly List<(float beat, Drum drum)> Drums = new List<(float, Drum)>();
        public float DrumVolume = 1f;
    }

    /// <summary>
    /// Tiny NES-style synthesizer: square/triangle voices plus noise drums.
    /// Renders a Song to a mono float buffer. Note tails wrap around, so the result loops seamlessly.
    /// </summary>
    public static class ChiptuneSynth
    {
        const float Attack = 0.005f;
        const float Release = 0.03f;

        public static float[] Render(Song song, int sampleRate)
        {
            double samplesPerBeat = sampleRate * 60.0 / song.Bpm;
            int length = (int)Math.Round(song.Beats * samplesPerBeat);
            var buffer = new float[length];

            foreach (SynthTrack track in song.Tracks)
                foreach (SynthNote note in track.Notes)
                    RenderNote(buffer, track, note, samplesPerBeat, sampleRate);

            var rng = new Random(7);
            foreach ((float beat, Drum drum) in song.Drums)
                RenderDrum(buffer, drum, (int)(beat * samplesPerBeat), sampleRate, song.DrumVolume, rng);

            // Soft clip, then normalize.
            float peak = 0f;
            for (int i = 0; i < length; i++)
            {
                buffer[i] = (float)Math.Tanh(buffer[i] * 1.2f);
                peak = Math.Max(peak, Math.Abs(buffer[i]));
            }
            if (peak > 0f)
                for (int i = 0; i < length; i++) buffer[i] *= 0.89f / peak;
            return buffer;
        }

        public static float MidiToHz(int midi) => 440f * (float)Math.Pow(2, (midi - 69) / 12.0);

        static void RenderNote(float[] buffer, SynthTrack track, SynthNote note, double samplesPerBeat, int sr)
        {
            int start = (int)(note.StartBeat * samplesPerBeat);
            int held = (int)(note.LengthBeats * samplesPerBeat);
            int tail = (int)(Release * sr);
            float baseHz = MidiToHz(note.Midi);
            double phase = 0;

            for (int i = 0; i < held + tail; i++)
            {
                float t = (float)i / sr;
                float env = t < Attack ? t / Attack : 1f;
                if (track.Pluck) env *= (float)Math.Exp(-t * 18f);
                else if (track.Wave == Wave.Triangle) env *= 0.5f + 0.5f * (float)Math.Exp(-t * 4f);
                else env *= 0.6f + 0.4f * (float)Math.Exp(-t * 10f);
                if (i >= held) env *= 1f - (float)(i - held) / tail;

                float hz = baseHz;
                if (track.Vibrato && t > 0.15f) hz *= 1f + 0.006f * (float)Math.Sin(2 * Math.PI * 5.5 * t);
                phase += hz / sr;
                float p = (float)(phase - Math.Floor(phase));

                float s = track.Wave switch
                {
                    Wave.Square25 => p < 0.25f ? 1f : -1f,
                    Wave.Square12 => p < 0.125f ? 1f : -1f,
                    _ => 4f * Math.Abs(p - 0.5f) - 1f,
                };
                buffer[(start + i) % buffer.Length] += s * env * track.Volume;
            }
        }

        static void RenderDrum(float[] buffer, Drum drum, int start, int sr, float volume, Random rng)
        {
            float duration = drum switch { Drum.Kick => 0.15f, Drum.Snare => 0.18f, _ => 0.04f };
            int n = (int)(duration * sr);
            double phase = 0;
            float prevNoise = 0f;

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float noise = (float)(rng.NextDouble() * 2 - 1);
                float s;
                switch (drum)
                {
                    case Drum.Kick:
                        phase += (40f + 80f * Math.Exp(-t * 30f)) / sr;
                        s = (float)Math.Sin(2 * Math.PI * phase) * (float)Math.Exp(-t * 20f) * 0.35f;
                        break;
                    case Drum.Snare:
                        s = (noise * 0.8f * (float)Math.Exp(-t * 18f)
                             + (float)Math.Sin(2 * Math.PI * 180 * t) * 0.4f * (float)Math.Exp(-t * 25f)) * 0.15f;
                        break;
                    default:
                        s = (noise - prevNoise) * (float)Math.Exp(-t * 90f) * 0.05f;
                        break;
                }
                prevNoise = noise;
                buffer[(start + i) % buffer.Length] += s * volume;
            }
        }
    }
}
