using UnityEngine;

namespace PixelAdventure.Stage
{
    /// <summary>Background layer that follows the camera by a fraction (1 = locked to camera, 0 = world-fixed).</summary>
    public class Parallax : MonoBehaviour
    {
        [SerializeField] Transform cam;
        [SerializeField, Range(0f, 1f)] float follow = 0.5f;

        float startX;
        float camStartX;

        void Start()
        {
            startX = transform.position.x;
            camStartX = cam.position.x;
        }

        void LateUpdate()
        {
            Vector3 p = transform.position;
            p.x = startX + (cam.position.x - camStartX) * follow;
            transform.position = p;
        }
    }
}
