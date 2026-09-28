using System.Collections;
using PixelAdventure.Combat;
using UnityEngine;

namespace PixelAdventure.Stage
{
    /// <summary>Straw dummy for testing combat: flashes, wobbles, slides from knockback, respawns after "death".</summary>
    [RequireComponent(typeof(Health), typeof(Rigidbody2D))]
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] Transform hpFill;
        [SerializeField] float respawnDelay = 2f;

        Health health;
        Rigidbody2D rb;
        Collider2D[] colliders;
        Vector3 spawnPos;
        float wobble;
        float flashUntil;

        void Awake()
        {
            health = GetComponent<Health>();
            rb = GetComponent<Rigidbody2D>();
            colliders = GetComponentsInChildren<Collider2D>();
            spawnPos = transform.position;
            health.Damaged += OnDamaged;
            health.Died += () => StartCoroutine(Respawn());
            health.Changed += (cur, max) => { if (hpFill != null) hpFill.localScale = new Vector3((float)cur / max, 1f, 1f); };
        }

        void OnDamaged(DamageInfo info)
        {
            rb.linearVelocity = info.Knockback;
            wobble = Mathf.Sign(info.Knockback.x) * 25f;
            flashUntil = Time.time + 0.08f;
        }

        void Update()
        {
            wobble = Mathf.Lerp(wobble, 0f, 1f - Mathf.Exp(-8f * Time.deltaTime));
            body.transform.localRotation = Quaternion.Euler(0, 0, -wobble);
            if (!health.IsDead) body.color = Time.time < flashUntil ? new Color(1f, 0.45f, 0.45f) : Color.white;
        }

        IEnumerator Respawn()
        {
            foreach (Collider2D c in colliders) c.enabled = false;
            rb.simulated = false;
            for (float t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                body.color = new Color(1f, 1f, 1f, 1f - t / 0.4f);
                yield return null;
            }
            body.color = Color.clear;
            yield return new WaitForSeconds(respawnDelay);

            transform.position = spawnPos;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = true;
            foreach (Collider2D c in colliders) c.enabled = true;
            health.ResetHealth();
            body.color = Color.white;
        }
    }
}
