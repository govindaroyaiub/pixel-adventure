using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.Anim
{
    /// <summary>Plays clips from a SpriteAnimationSet on a SpriteRenderer.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] SpriteAnimationSet set;

        class RuntimeClip
        {
            public Sprite[] Frames;
            public float Fps;
            public bool Loop;
        }

        readonly Dictionary<string, RuntimeClip> clips = new Dictionary<string, RuntimeClip>();
        SpriteRenderer spriteRenderer;
        RuntimeClip current;
        float time;
        float fps;
        bool manual;

        public string CurrentClip { get; private set; }
        public SpriteRenderer Renderer => spriteRenderer;

        /// <summary>True once a non-looping clip has shown its last frame.</summary>
        public bool IsFinished => current != null && !current.Loop && time * fps >= current.Frames.Length;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (set == null) return;
            foreach (SpriteAnimationSet.Clip clip in set.clips)
            {
                if (clip.texture == null) continue;
                int count = Mathf.Max(1, clip.texture.width / set.frameWidth);
                var frames = new Sprite[count];
                for (int i = 0; i < count; i++)
                {
                    var rect = new Rect(i * set.frameWidth, 0, set.frameWidth, set.frameHeight);
                    frames[i] = Sprite.Create(clip.texture, rect, set.pivot, set.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    frames[i].name = $"{clip.name}_{i}";
                }
                clips[clip.name] = new RuntimeClip { Frames = frames, Fps = clip.fps, Loop = clip.loop };
            }
        }

        public int FrameCount(string clip) => clips.TryGetValue(clip, out RuntimeClip c) ? c.Frames.Length : 0;

        /// <summary>Plays a clip. duration > 0 stretches the clip to last exactly that long.</summary>
        public void Play(string clip, bool restart = false, float duration = -1f)
        {
            if (!clips.TryGetValue(clip, out RuntimeClip c)) return;
            if (!restart && !manual && CurrentClip == clip) return;
            current = c;
            CurrentClip = clip;
            manual = false;
            time = 0f;
            fps = duration > 0f ? c.Frames.Length / duration : c.Fps;
            spriteRenderer.sprite = c.Frames[0];
        }

        /// <summary>Shows one frame and stops auto-advancing (e.g. jump frames picked by vertical speed).</summary>
        public void SetFrame(string clip, int index)
        {
            if (!clips.TryGetValue(clip, out RuntimeClip c)) return;
            current = c;
            CurrentClip = clip;
            manual = true;
            spriteRenderer.sprite = c.Frames[Mathf.Clamp(index, 0, c.Frames.Length - 1)];
        }

        void Update()
        {
            if (current == null || manual) return;
            time += Time.deltaTime;
            int frame = Mathf.FloorToInt(time * fps);
            frame = current.Loop ? frame % current.Frames.Length : Mathf.Min(frame, current.Frames.Length - 1);
            spriteRenderer.sprite = current.Frames[frame];
        }
    }
}
