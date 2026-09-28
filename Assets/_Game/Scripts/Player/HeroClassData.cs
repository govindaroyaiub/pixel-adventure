using System;
using PixelAdventure.Anim;
using UnityEngine;

namespace PixelAdventure.Player
{
    [Serializable]
    public class AttackData
    {
        public string animation = "Attack1";
        public int damage = 10;
        public float windup = 0.08f;
        public float active = 0.1f;
        public float recovery = 0.2f;
        [Tooltip("Hitbox center relative to the feet, x = forward.")]
        public Vector2 hitOffset = new Vector2(0.8f, 0.75f);
        public Vector2 hitSize = new Vector2(1.4f, 1.2f);
        public Vector2 knockback = new Vector2(3f, 1f);
        [Tooltip("Forward speed during windup (small step into the swing).")]
        public float lunge = 1.5f;

        public float Total => windup + active + recovery;
    }

    [Serializable]
    public class BashData
    {
        public int damage = 8;
        public float cooldown = 5f;
        public float windup = 0.1f;
        public float dashTime = 0.25f;
        public float dashSpeed = 10f;
        public float recovery = 0.15f;
        public Vector2 hitOffset = new Vector2(0.6f, 0.75f);
        public Vector2 hitSize = new Vector2(1.1f, 1.4f);
        public Vector2 knockback = new Vector2(9f, 3f);
        public float stun = 1f;
    }

    [Serializable]
    public class SlamData
    {
        public int damage = 40;
        public float cooldown = 30f;
        public float crouch = 0.15f;
        public float leapVelocity = 13f;
        public float maxAirTime = 0.45f;
        public float slamSpeed = 28f;
        public float radius = 4f;
        public Vector2 knockback = new Vector2(10f, 6f);
        public float stun = 0.6f;
        public float recovery = 0.35f;
    }

    /// <summary>
    /// Everything tunable about a hero class. New class = new asset (+ its skill code).
    /// `id` + `unlockedByDefault` are the hooks for buying characters later.
    /// </summary>
    [CreateAssetMenu(menuName = "Pixel Adventure/Hero Class")]
    public class HeroClassData : ScriptableObject
    {
        public string id = "knight";
        public string displayName = "Knight";
        public bool unlockedByDefault = true;
        public SpriteAnimationSet animations;

        [Header("Stats")]
        public int maxHp = 100;
        public float moveSpeed = 5f;

        [Header("Jump")]
        public float jumpVelocity = 15f;
        public float gravity = 40f;
        public float fallGravityMultiplier = 1.6f;
        public float maxFallSpeed = 22f;
        public float coyoteTime = 0.1f;
        public float jumpBuffer = 0.1f;

        [Header("Roll")]
        public float rollSpeed = 9f;
        public float rollDuration = 0.4f;
        public float rollCooldown = 1f;

        [Header("Attack combo")]
        public AttackData[] combo =
        {
            new AttackData { animation = "Attack1", damage = 10, windup = 0.08f, active = 0.1f, recovery = 0.2f },
            new AttackData { animation = "Attack2", damage = 10, windup = 0.06f, active = 0.1f, recovery = 0.2f },
            new AttackData
            {
                animation = "Attack3", damage = 15, windup = 0.12f, active = 0.12f, recovery = 0.3f,
                hitOffset = new Vector2(0.9f, 0.8f), hitSize = new Vector2(1.7f, 1.4f), knockback = new Vector2(7f, 3f), lunge = 2.5f,
            },
        };
        public float comboResetTime = 0.6f;

        [Header("Primary: Shield Bash")]
        public BashData bash = new BashData();

        [Header("Ultra: Holy Slam")]
        public SlamData slam = new SlamData();
    }
}
