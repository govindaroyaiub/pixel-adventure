using UnityEngine;

namespace PixelAdventure.Audio
{
    /// <summary>Loops a music clip with a short fade-in.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] AudioClip clip;
        [SerializeField, Range(0f, 1f)] float volume = 0.6f;
        [SerializeField] float fadeIn = 1.5f;

        AudioSource source;

        void Start()
        {
            source = GetComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            if (clip != null) source.Play();
        }

        void Update()
        {
            if (source.volume < volume)
                source.volume = Mathf.MoveTowards(source.volume, volume, volume / fadeIn * Time.unscaledDeltaTime);
        }
    }
}
