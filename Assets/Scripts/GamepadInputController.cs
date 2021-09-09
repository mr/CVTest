using UnityEngine.InputSystem;

public class GamepadInputController : IInputController {
    private Gamepad gamepad;
    private Gamepad Current {
        get {
            if (gamepad == null) {
                gamepad = Gamepad.current;
            }

            return gamepad;
        }
    }

    public float left { get => -1 * (Current?.dpad?.left?.ReadValue() ?? 0); }
    public float up { get => Current?.dpad?.up?.ReadValue() ?? 0; }
    public float right { get => Current?.dpad?.right?.ReadValue() ?? 0; }
    public float down { get => -1 * (Current?.dpad?.down?.ReadValue() ?? 0); }

    public bool jump { get => Current?.aButton?.wasPressedThisFrame ?? false; }

    public bool attack { get => Current?.xButton?.wasPressedThisFrame ?? false; }
}