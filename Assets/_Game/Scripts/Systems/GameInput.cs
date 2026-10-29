using UnityEngine;
using UnityEngine.InputSystem;

namespace ExitClause.Systems
{
    /// <summary>
    /// All player input, defined in code with the new Input System (keyboard/mouse plus gamepad).
    /// UI screens call PushBlock/PopBlock so gameplay ignores input while menus are open.
    /// </summary>
    public static class GameInput
    {
        public static InputAction Move, Look, Sprint, Crouch, Jump, Interact, Aim, Fire, Reload, Melee, Cover,
            Gadget, CycleGadget, NextWeapon, Weapon1, Weapon2, Weapon3, Rage, Journal, Pause, TopDown, LookBehind,
            Handbrake, Skip, CycleTarget;

        static int s_blockCount;
        static bool s_created;

        /// <summary>True while a menu or minigame owns the input.</summary>
        public static bool GameplayBlocked => s_blockCount > 0;

        public static void PushBlock() { s_blockCount++; RefreshCursor(); }
        public static void PopBlock() { s_blockCount = Mathf.Max(0, s_blockCount - 1); RefreshCursor(); }
        public static void ResetBlocks() { s_blockCount = 0; RefreshCursor(); }

        public static void RefreshCursor()
        {
            bool free = s_blockCount > 0 || ForceCursorVisible;
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
        }

        /// <summary>Set by the main menu scene so the cursor stays usable.</summary>
        public static bool ForceCursorVisible;

        public static void Enable()
        {
            if (!s_created) Create();
        }

        static InputAction Button(string name, params string[] bindings)
        {
            var a = new InputAction(name, InputActionType.Button);
            foreach (var b in bindings) a.AddBinding(b);
            a.Enable();
            return a;
        }

        static void Create()
        {
            s_created = true;

            Move = new InputAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddBinding("<Gamepad>/leftStick");
            Move.Enable();

            Look = new InputAction("Look", InputActionType.Value);
            Look.AddBinding("<Mouse>/delta").WithProcessor("scaleVector2(x=0.05,y=0.05)");
            Look.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=4,y=4)");
            Look.Enable();

            Sprint = Button("Sprint", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            Crouch = Button("Crouch", "<Keyboard>/c", "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");
            Jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            Interact = Button("Interact", "<Keyboard>/e", "<Gamepad>/buttonWest");
            Aim = Button("Aim", "<Mouse>/rightButton", "<Gamepad>/leftTrigger");
            Fire = Button("Fire", "<Mouse>/leftButton", "<Gamepad>/rightTrigger");
            Reload = Button("Reload", "<Keyboard>/r", "<Gamepad>/buttonNorth");
            Melee = Button("Melee", "<Keyboard>/f", "<Gamepad>/rightStickPress");
            Cover = Button("Cover", "<Keyboard>/q", "<Gamepad>/rightShoulder");
            Gadget = Button("Gadget", "<Keyboard>/g", "<Gamepad>/leftShoulder");
            CycleGadget = Button("CycleGadget", "<Keyboard>/h", "<Gamepad>/dpad/left");
            NextWeapon = Button("NextWeapon", "<Mouse>/scroll/up", "<Gamepad>/dpad/right");
            Weapon1 = Button("Weapon1", "<Keyboard>/1");
            Weapon2 = Button("Weapon2", "<Keyboard>/2");
            Weapon3 = Button("Weapon3", "<Keyboard>/3");
            Rage = Button("Rage", "<Keyboard>/v", "<Gamepad>/dpad/up");
            Journal = Button("Journal", "<Keyboard>/j", "<Keyboard>/tab", "<Gamepad>/select");
            Pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            TopDown = Button("TopDown", "<Keyboard>/t", "<Gamepad>/dpad/down");
            LookBehind = Button("LookBehind", "<Keyboard>/b", "<Gamepad>/rightStickPress");
            Handbrake = Button("Handbrake", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            Skip = Button("Skip", "<Keyboard>/enter", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            CycleTarget = Button("CycleTarget", "<Mouse>/middleButton", "<Keyboard>/x", "<Gamepad>/rightStickPress");
        }

        /// <summary>Gameplay-safe helpers: return false/zero while a UI block is active.</summary>
        public static bool Pressed(InputAction a) => !GameplayBlocked && a != null && a.WasPressedThisFrame();
        public static bool Held(InputAction a) => !GameplayBlocked && a != null && a.IsPressed();
        public static Vector2 Vector(InputAction a) => GameplayBlocked || a == null ? Vector2.zero : a.ReadValue<Vector2>();
    }
}
