using System.Collections.Generic;
using UnityEngine;

namespace PixelAdventure.Combat
{
    /// <summary>Marks a collider as hittable. Damage from the same team is ignored.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hurtbox : MonoBehaviour
    {
        [SerializeField] Team team;
        [SerializeField] Health health;

        public Team Team => team;
        public Health Health => health;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
        }

        public bool Receive(DamageInfo info) => info.SourceTeam != team && health != null && health.TakeDamage(info);
    }

    /// <summary>Overlap queries that deliver damage to every Hurtbox in an area, once per attack.</summary>
    public static class HitQuery
    {
        static readonly List<Collider2D> Results = new List<Collider2D>(16);

        public static int Box(Vector2 center, Vector2 size, DamageInfo info, HashSet<Hurtbox> alreadyHit)
        {
            Physics2D.OverlapBox(center, size, 0f, Filter(), Results);
            return Deliver(info, alreadyHit);
        }

        public static int Circle(Vector2 center, float radius, DamageInfo info, HashSet<Hurtbox> alreadyHit)
        {
            Physics2D.OverlapCircle(center, radius, Filter(), Results);
            return Deliver(info, alreadyHit);
        }

        static ContactFilter2D Filter()
        {
            var filter = new ContactFilter2D();
            filter.NoFilter();
            return filter;
        }

        static int Deliver(DamageInfo info, HashSet<Hurtbox> alreadyHit)
        {
            int hits = 0;
            foreach (Collider2D col in Results)
            {
                var hurtbox = col.GetComponent<Hurtbox>();
                if (hurtbox == null || alreadyHit.Contains(hurtbox)) continue;

                // Knockback always pushes away from the attacker.
                var directed = info;
                float side = Mathf.Sign(col.bounds.center.x - info.SourcePosition.x);
                directed.Knockback = new Vector2(Mathf.Abs(info.Knockback.x) * side, info.Knockback.y);

                if (hurtbox.Receive(directed))
                {
                    alreadyHit.Add(hurtbox);
                    hits++;
                }
            }
            return hits;
        }
    }
}
