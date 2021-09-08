using UnityEngine.InputSystem;

public class KeyboardInputController : IInputController {
    private Keyboard keyboard;

    public float left { get => keyboard.hKey.isPressed ? -1 : 0; }
    public float up { get => keyboard.kKey.isPressed ? 1 : 0; }
    public float right { get => keyboard.lKey.isPressed ? 1 : 0; }
    public float down { get => keyboard.jKey.isPressed ? -1 : 0; }

    public bool jump { get => keyboard.zKey.wasPressedThisFrame; }

    public bool attack { get => keyboard.xKey.wasPressedThisFrame; }

    public KeyboardInputController(Keyboard keyboard) {
        this.keyboard = keyboard;
    }
}