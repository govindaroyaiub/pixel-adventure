using System;
using UnityEngine;

namespace PixelAdventure.Combat
{
    public enum Team { Player, Enemy }

    public struct DamageInfo
    {
        public int Amount;
        public Vector2 Knockback;
        public float Stun;
        public Team SourceTeam;
        public Vector2 SourcePosition;
    }

    /// <summary>Hit points for anything that can be damaged (player, enemies, dummies, boss).</summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] int max = 100;

        int current;

        public int Max => max;
        public int Current => current;
        public bool IsDead => current <= 0;
        public bool Invulnerable { get; set; }

        public event Action<DamageInfo> Damaged;
        public event Action Died;
        public event Action<int, int> Changed; // current, max

        void Awake() => current = max;

        public void SetMax(int value, bool refill)
        {
            max = Mathf.Max(1, value);
            current = refill ? max : Mathf.Min(current, max);
            Changed?.Invoke(current, max);
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (IsDead || Invulnerable || info.Amount <= 0) return false;
            current = Mathf.Max(0, current - info.Amount);
            Changed?.Invoke(current, max);
            Damaged?.Invoke(info);
            if (current == 0) Died?.Invoke();
            return true;
        }

        public void ResetHealth()
        {
            current = max;
            Changed?.Invoke(current, max);
        }
    }
}
