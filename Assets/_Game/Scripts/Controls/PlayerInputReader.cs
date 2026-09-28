using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelAdventure.Controls
{
    /// <summary>
    /// Merges touch controls and keyboard into one set of per-frame values.
    /// Jump = flick the stick up past the threshold (must drop back before the next jump), or W / Space / Up.
    /// Keyboard: A/D or arrows move, J attack, K roll, L primary, I ultra.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] VirtualJoystick joystick;
        [SerializeField] TouchButton attackButton;
        [SerializeField] TouchButton rollButton;
        [SerializeField] TouchButton primaryButton;
        [SerializeField] TouchButton ultraButton;
        [SerializeField, Range(0.3f, 0.95f)] float flickThreshold = 0.6f;
        [SerializeField, Range(0.1f, 0.6f)] float moveDeadZone = 0.3f;

        bool stickWasUp;

        public float Move { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool RollPressed { get; private set; }
        public bool PrimaryPressed { get; private set; }
        public bool UltraPressed { get; private set; }

        public TouchButton RollButton => rollButton;
        public TouchButton PrimaryButton => primaryButton;
        public TouchButton UltraButton => ultraButton;

        void Update()
        {
            Keyboard kb = Keyboard.current;
            Vector2 stick = joystick != null ? joystick.Value : Vector2.zero;

            float move = 0f;
            if (Mathf.Abs(stick.x) > moveDeadZone) move = Mathf.Sign(stick.x); // digital, Metal Slug style
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += 1f;
            }
            Move = Mathf.Clamp(move, -1f, 1f);

            bool stickUp = stick.y > flickThreshold;
            bool flick = stickUp && !stickWasUp;
            if (stickUp) stickWasUp = true;
            else if (stick.y < flickThreshold * 0.5f) stickWasUp = false;

            JumpPressed = flick || Pressed(kb, Key.W) || Pressed(kb, Key.Space) || Pressed(kb, Key.UpArrow);
            AttackPressed = Consume(attackButton) | Pressed(kb, Key.J);
            RollPressed = Consume(rollButton) | Pressed(kb, Key.K);
            PrimaryPressed = Consume(primaryButton) | Pressed(kb, Key.L);
            UltraPressed = Consume(ultraButton) | Pressed(kb, Key.I);
        }

        static bool Pressed(Keyboard kb, Key key) => kb != null && kb[key].wasPressedThisFrame;
        static bool Consume(TouchButton button) => button != null && button.ConsumePress();
    }
}
