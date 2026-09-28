using System.Collections;
using PixelAdventure.Combat;
using PixelAdventure.Controls;
using PixelAdventure.Core;
using UnityEngine;

namespace PixelAdventure.Player
{
    /// <summary>
    /// Run, jump (coyote time + buffer, heavier fall), roll with i-frames.
    /// Uses a dynamic Rigidbody2D with our own gravity so movement stays tight.
    /// PlayerCombat takes over movement while an attack or skill runs (Locked).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] HeroClassData data;
        [SerializeField] PlayerInputReader input;
        [SerializeField] LayerMask groundMask;

        Rigidbody2D rb;
        BoxCollider2D box;
        Health health;
        Cooldown rollCooldown;
        float lastGroundedTime = -99f;
        float jumpPressedTime = -99f;

        public HeroClassData Data => data;
        public PlayerInputReader Input => input;
        public int Facing { get; private set; } = 1;
        public bool Grounded { get; private set; }
        public bool IsRolling { get; private set; }
        public float RollProgress { get; private set; }
        public bool Locked { get; set; }
        public float GravityScale { get; set; } = 1f;
        public Vector2 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }
        public Cooldown RollCooldown => rollCooldown;

        /// <summary>World x the player can't move left of (camera's left edge).</summary>
        public float LeftLimit { get; set; } = float.MinValue;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            health = GetComponent<Health>();
            if (data == null || input == null)
            {
                Debug.LogError("PlayerMotor: hero data or input is missing. Run Pixel Adventure → Build Stage 1-1.", this);
                enabled = false;
                return;
            }
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rollCooldown = new Cooldown(data.rollCooldown);
        }

        void Update()
        {
            if (health != null && health.IsDead) return;
            if (input.JumpPressed) jumpPressedTime = Time.time;
            if (input.RollPressed && CanRoll()) StartCoroutine(Roll());
        }

        void FixedUpdate()
        {
            Vector2 vel = rb.linearVelocity;
            Grounded = vel.y <= 0.1f && CheckGround();
            if (Grounded) lastGroundedTime = Time.time;

            bool dead = health != null && health.IsDead;
            if (!Locked && !IsRolling && !dead)
            {
                vel.x = input.Move * data.moveSpeed;
                if (input.Move != 0f) Facing = input.Move > 0f ? 1 : -1;

                bool buffered = Time.time - jumpPressedTime <= data.jumpBuffer;
                bool coyote = Time.time - lastGroundedTime <= data.coyoteTime;
                if (buffered && coyote)
                {
                    vel.y = data.jumpVelocity;
                    jumpPressedTime = -99f;
                    lastGroundedTime = -99f;
                    Grounded = false;
                }
            }
            else if (dead)
            {
                vel.x = Mathf.MoveTowards(vel.x, 0f, 30f * Time.fixedDeltaTime);
            }

            float g = data.gravity * GravityScale * (vel.y < 0f ? data.fallGravityMultiplier : 1f);
            vel.y = Mathf.Max(vel.y - g * Time.fixedDeltaTime, -data.maxFallSpeed);
            if (Grounded && vel.y < 0f) vel.y = -1f; // stay glued to the ground

            float halfWidth = box.size.x * 0.5f;
            if (rb.position.x - halfWidth < LeftLimit)
            {
                rb.position = new Vector2(LeftLimit + halfWidth, rb.position.y);
                vel.x = Mathf.Max(0f, vel.x);
            }

            rb.linearVelocity = vel;
        }

        public void Face(int direction)
        {
            if (direction != 0) Facing = direction > 0 ? 1 : -1;
        }

        bool CanRoll() => Grounded && !Locked && !IsRolling && rollCooldown.IsReady(Time.time);

        IEnumerator Roll()
        {
            IsRolling = true;
            if (health != null) health.Invulnerable = true;
            rollCooldown.Trigger(Time.time);
            if (input.Move != 0f) Facing = input.Move > 0f ? 1 : -1;

            float t = 0f;
            while (t < data.rollDuration)
            {
                RollProgress = t / data.rollDuration;
                rb.linearVelocity = new Vector2(Facing * data.rollSpeed, rb.linearVelocity.y);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }

            RollProgress = 0f;
            IsRolling = false;
            if (health != null) health.Invulnerable = false;
        }

        bool CheckGround()
        {
            Bounds b = box.bounds;
            var origin = new Vector2(b.center.x, b.min.y + 0.05f);
            var size = new Vector2(b.size.x * 0.9f, 0.05f);
            return Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.1f, groundMask).collider != null;
        }
    }
}
