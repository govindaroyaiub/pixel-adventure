using System;
using System.Collections;
using System.Collections.Generic;
using PixelAdventure.Anim;
using PixelAdventure.Combat;
using PixelAdventure.Core;
using PixelAdventure.Effects;
using PixelAdventure.Stage;
using UnityEngine;

namespace PixelAdventure.Player
{
    /// <summary>
    /// Knight actions: 3-hit sword combo, Shield Bash (primary), Holy Slam (ultra).
    /// Also picks the idle/run/jump/roll animation when no action is running.
    /// Each action has windup → active → recovery; the hitbox only exists during active.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor), typeof(Health))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] SpriteAnimator animator;
        [SerializeField] Sprite slamRing;

        static readonly Color DashTrail = new Color(0.55f, 0.75f, 1f, 0.6f);
        static readonly Color SlamGold = new Color(1f, 0.85f, 0.3f, 0.9f);

        PlayerMotor motor;
        Health health;
        HeroClassData data;
        Cooldown bashCooldown;
        Cooldown slamCooldown;
        readonly HashSet<Hurtbox> hitThisAction = new HashSet<Hurtbox>();

        Coroutine action;
        bool attacking;
        bool attackQueued;
        int comboIndex = -1;
        float lastAttackEnd = -99f;
        float trailTimer;
        float hurtUntil;

        public bool Busy => action != null;
        public event Action Died;

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            health = GetComponent<Health>();
            data = motor.Data;
            bashCooldown = new Cooldown(data.bash.cooldown);
            slamCooldown = new Cooldown(data.slam.cooldown);
            health.SetMax(data.maxHp, true);
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        void Update()
        {
            if (health.IsDead) return;
            var input = motor.Input;

            if (input.AttackPressed)
            {
                if (attacking) attackQueued = true;
                else if (!Busy && !motor.IsRolling) action = StartCoroutine(AttackChain());
            }
            if (input.PrimaryPressed && !Busy && !motor.IsRolling && bashCooldown.IsReady(Time.time))
                action = StartCoroutine(ShieldBash());
            if (input.UltraPressed && !Busy && !motor.IsRolling && motor.Grounded && slamCooldown.IsReady(Time.time))
                action = StartCoroutine(HolySlam());

            if (input.RollButton != null) input.RollButton.SetCooldown(motor.RollCooldown.Remaining01(Time.time));
            if (input.PrimaryButton != null) input.PrimaryButton.SetCooldown(bashCooldown.Remaining01(Time.time));
            if (input.UltraButton != null) input.UltraButton.SetCooldown(slamCooldown.Remaining01(Time.time));
        }

        void LateUpdate()
        {
            animator.Renderer.flipX = motor.Facing < 0;
            if (health.IsDead || Busy) return;

            if (motor.IsRolling)
            {
                animator.SetFrame("Jump", 2);
                animator.transform.localRotation = Quaternion.Euler(0, 0, -motor.Facing * 360f * motor.RollProgress);
                Trail(DashTrail, 0.03f);
                return;
            }
            animator.transform.localRotation = Quaternion.identity;

            if (Time.time < hurtUntil) { animator.Play("Hurt"); return; }

            if (!motor.Grounded)
            {
                float vy = motor.Velocity.y;
                animator.SetFrame("Jump", vy > 6f ? 1 : vy > 0f ? 2 : vy > -6f ? 3 : 4);
            }
            else if (Mathf.Abs(motor.Velocity.x) > 0.1f) animator.Play("Run");
            else animator.Play("Idle");
        }

        // ---------- Sword combo ----------

        IEnumerator AttackChain()
        {
            attacking = true;
            motor.Locked = true;
            int index = Time.time - lastAttackEnd <= data.comboResetTime ? (comboIndex + 1) % data.combo.Length : 0;

            while (true)
            {
                comboIndex = index;
                attackQueued = false;
                AttackData a = data.combo[index];
                if (motor.Input.Move != 0f) motor.Face((int)Mathf.Sign(motor.Input.Move));
                animator.transform.localRotation = Quaternion.identity;
                animator.Play(a.animation, true, a.Total);
                hitThisAction.Clear();

                yield return Phase(a.windup, () => SetHorizontal(motor.Grounded ? motor.Facing * a.lunge : motor.Velocity.x));
                yield return Phase(a.active, () =>
                {
                    SetHorizontal(motor.Grounded ? 0f : motor.Velocity.x);
                    int hits = HitQuery.Box(Forward(a.hitOffset), a.hitSize, Damage(a.damage, a.knockback, 0.2f), hitThisAction);
                    if (hits > 0) HitPause.Trigger(0.05f);
                });

                // A queued press skips recovery and chains into the next swing.
                float t = 0f;
                while (t < a.recovery && !attackQueued)
                {
                    SetHorizontal(motor.Grounded ? 0f : motor.Velocity.x);
                    yield return null;
                    t += Time.deltaTime;
                }
                if (!attackQueued || index == data.combo.Length - 1) break;
                index++;
            }

            lastAttackEnd = Time.time;
            if (comboIndex == data.combo.Length - 1) comboIndex = -1;
            EndAction();
        }

        // ---------- Primary: Shield Bash ----------

        IEnumerator ShieldBash()
        {
            BashData b = data.bash;
            motor.Locked = true;
            bashCooldown.Trigger(Time.time);
            if (motor.Input.Move != 0f) motor.Face((int)Mathf.Sign(motor.Input.Move));
            animator.Play("Defend", true, b.windup + b.dashTime + b.recovery);
            hitThisAction.Clear();

            yield return Phase(b.windup, () => SetHorizontal(0f));
            yield return Phase(b.dashTime, () =>
            {
                SetHorizontal(motor.Facing * b.dashSpeed);
                Trail(DashTrail, 0.04f);
                int hits = HitQuery.Box(Forward(b.hitOffset), b.hitSize, Damage(b.damage, b.knockback, b.stun), hitThisAction);
                if (hits > 0) { HitPause.Trigger(0.06f); StageCamera.Shake(0.15f, 0.15f); }
            });
            yield return Phase(b.recovery, () => SetHorizontal(0f));
            EndAction();
        }

        // ---------- Ultra: Holy Slam ----------

        IEnumerator HolySlam()
        {
            SlamData s = data.slam;
            motor.Locked = true;
            health.Invulnerable = true;
            slamCooldown.Trigger(Time.time);

            animator.SetFrame("Jump", 0);
            yield return Phase(s.crouch, () => SetHorizontal(0f));

            motor.Velocity = new Vector2(motor.Facing * 2f, s.leapVelocity);
            float air = 0f;
            while (air < s.maxAirTime && motor.Velocity.y > 0f)
            {
                animator.SetFrame("Jump", 1);
                yield return new WaitForFixedUpdate();
                air += Time.fixedDeltaTime;
            }

            motor.GravityScale = 0f;
            float fall = 0f;
            while (!motor.Grounded && fall < 1f)
            {
                motor.Velocity = new Vector2(0f, -s.slamSpeed);
                animator.SetFrame("Jump", 4);
                Trail(SlamGold, 0.03f);
                yield return new WaitForFixedUpdate();
                fall += Time.fixedDeltaTime;
            }
            motor.GravityScale = 1f;

            Vector2 impact = transform.position;
            animator.Play("Attack3", true, s.recovery + 0.15f);
            hitThisAction.Clear();
            HitQuery.Circle(impact + Vector2.up * 0.5f, s.radius, Damage(s.damage, s.knockback, s.stun), hitThisAction);
            Shockwave.Spawn(slamRing, impact + Vector2.up * 0.2f, s.radius, SlamGold);
            StageCamera.Shake(0.4f, 0.35f);
            HitPause.Trigger(0.08f);

            yield return Phase(s.recovery, () => SetHorizontal(0f));
            health.Invulnerable = false;
            EndAction();
        }

        // ---------- Helpers ----------

        IEnumerator Phase(float duration, Action tick)
        {
            float t = 0f;
            while (t < duration)
            {
                tick();
                yield return null;
                t += Time.deltaTime;
            }
        }

        void EndAction()
        {
            attacking = false;
            attackQueued = false;
            motor.Locked = false;
            motor.GravityScale = 1f;
            action = null;
        }

        void SetHorizontal(float vx) => motor.Velocity = new Vector2(vx, motor.Velocity.y);

        Vector2 Forward(Vector2 offset) => (Vector2)transform.position + new Vector2(offset.x * motor.Facing, offset.y);

        DamageInfo Damage(int amount, Vector2 knockback, float stun) => new DamageInfo
        {
            Amount = amount,
            Knockback = knockback,
            Stun = stun,
            SourceTeam = Team.Player,
            SourcePosition = transform.position,
        };

        void Trail(Color tint, float interval)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer > 0f) return;
            trailTimer = interval;
            AfterImage.Spawn(animator.Renderer, tint);
        }

        void OnDamaged(DamageInfo info)
        {
            if (Busy) return;
            hurtUntil = Time.time + 0.25f;
            motor.Velocity = info.Knockback;
        }

        void OnDied()
        {
            if (action != null) StopCoroutine(action);
            EndAction();
            motor.Locked = true;
            animator.transform.localRotation = Quaternion.identity;
            animator.Play("Death", true);
            Died?.Invoke();
        }

        void OnDrawGizmosSelected()
        {
            if (motor == null || data == null) return;
            Gizmos.color = Color.red;
            foreach (AttackData a in data.combo) Gizmos.DrawWireCube(Forward(a.hitOffset), a.hitSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, data.slam.radius);
        }
    }
}
