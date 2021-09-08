using UnityEngine.InputSystem;

public class GamepadInputController : IInputController {
    private Gamepad gamepad;

    public float left { get => -1 * gamepad.dpad.left.ReadValue(); }
    public float up { get => gamepad.dpad.up.ReadValue(); }
    public float right { get => gamepad.dpad.right.ReadValue(); }
    public float down { get => -1 * gamepad.dpad.down.ReadValue(); }

    public bool jump { get => gamepad.aButton.wasPressedThisFrame; }

    public bool attack { get => gamepad.xButton.wasPressedThisFrame; }

    public GamepadInputController(Gamepad gamepad) {
        this.gamepad = gamepad;
    }
}