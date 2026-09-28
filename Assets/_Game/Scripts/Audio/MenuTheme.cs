using System;
using System.Collections.Generic;

namespace PixelAdventure.Audio
{
    /// <summary>
    /// "Pixel Adventure" main menu theme: original 16-bar heroic loop in A minor, 132 BPM.
    /// Progression: Am F C G | Am F G E, played twice with a varied melody the second time.
    /// </summary>
    public static class MenuTheme
    {
        // Melody in 8th-note lengths, one bar per entry. R = rest.
        static readonly string[] Melody =
        {
            "A4:2 C5:2 E5:3 D5:1", "C5:2 A4:2 F4:2 A4:2", "G4:2 C5:2 E5:2 G5:2", "F5:3 E5:1 D5:4",
            "E5:2 A5:2 G5:2 E5:2", "F5:3 E5:1 C5:2 A4:2", "B4:2 D5:2 G5:2 F5:2", "E5:6 R:2",
            "A5:3 G5:1 E5:2 C5:2", "D5:2 C5:2 A4:4",      "E5:2 G5:2 C6:3 B5:1", "A5:2 G5:2 D5:4",
            "C5:1 D5:1 E5:2 A5:2 E5:2", "F5:2 A5:2 G5:2 F5:2", "D5:2 B4:2 G4:2 B4:2", "G#4:4 B4:2 E5:2",
        };

        static readonly string[] Chords = { "Am", "F", "C", "G", "Am", "F", "G", "E" };

        // Bass root (octave 2-3) and arpeggio tones (octave 3-4) per chord.
        static readonly Dictionary<string, (int bass, int[] arp)> ChordTones = new Dictionary<string, (int, int[])>
        {
            ["Am"] = (45, new[] { 57, 60, 64 }),
            ["F"] = (41, new[] { 53, 57, 60 }),
            ["C"] = (48, new[] { 55, 60, 64 }),
            ["G"] = (43, new[] { 55, 59, 62 }),
            ["E"] = (40, new[] { 52, 56, 59 }),
        };

        public static Song Build()
        {
            const int bars = 16;
            var song = new Song { Bpm = 132, Beats = bars * 4 };
            var lead = new SynthTrack { Wave = Wave.Square25, Volume = 0.22f, Vibrato = true };
            var arp = new SynthTrack { Wave = Wave.Square12, Volume = 0.08f, Pluck = true };
            var bass = new SynthTrack { Wave = Wave.Triangle, Volume = 0.25f };
            song.Tracks.Add(lead);
            song.Tracks.Add(arp);
            song.Tracks.Add(bass);

            int[] arpPattern = { 0, 1, 2, 1 };
            int[] bassPattern = { 0, 0, 12, 0, 0, 0, 12, 7 };

            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4f;

                float beat = barStart;
                foreach (string token in Melody[bar].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = token.Split(':');
                    float len = int.Parse(parts[1]) * 0.5f;
                    if (parts[0] != "R") lead.Notes.Add(new SynthNote { StartBeat = beat, LengthBeats = len * 0.95f, Midi = NoteToMidi(parts[0]) });
                    beat += len;
                }

                var (root, tones) = ChordTones[Chords[bar % Chords.Length]];
                for (int i = 0; i < 16; i++)
                    arp.Notes.Add(new SynthNote { StartBeat = barStart + i * 0.25f, LengthBeats = 0.2f, Midi = tones[arpPattern[i % 4]] });
                for (int i = 0; i < 8; i++)
                    bass.Notes.Add(new SynthNote { StartBeat = barStart + i * 0.5f, LengthBeats = 0.45f, Midi = root + bassPattern[i] });

                song.Drums.Add((barStart, Drum.Kick));
                song.Drums.Add((barStart + 2f, Drum.Kick));
                if (bar % 2 == 1) song.Drums.Add((barStart + 2.5f, Drum.Kick));
                song.Drums.Add((barStart + 1f, Drum.Snare));
                song.Drums.Add((barStart + 3f, Drum.Snare));
                for (int i = 0; i < 8; i++) song.Drums.Add((barStart + i * 0.5f, Drum.Hat));
            }
            return song;
        }

        static int NoteToMidi(string note)
        {
            string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string name = note.Substring(0, note.Length - 1);
            int octave = note[note.Length - 1] - '0';
            return 12 * (octave + 1) + Array.IndexOf(names, name);
        }
    }
}
