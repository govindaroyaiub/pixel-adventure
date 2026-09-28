using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.Anim
{
    /// <summary>
    /// Sprite-sheet animations for one character. Each clip is a horizontal strip of equal-size frames.
    /// Swapping art = pointing clips at new textures; gameplay code only uses clip names.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixel Adventure/Sprite Animation Set")]
    public class SpriteAnimationSet : ScriptableObject
    {
        [Serializable]
        public class Clip
        {
            public string name;
            public Texture2D texture;
            public float fps = 10f;
            public bool loop = true;
        }

        public int frameWidth = 96;
        public int frameHeight = 84;
        [Tooltip("Pivot in frame space (0-1). Should sit at the character's body center.")]
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public float pixelsPerUnit = 24f;
        public List<Clip> clips = new List<Clip>();
    }
}
