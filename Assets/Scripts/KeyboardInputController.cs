using UnityEngine.InputSystem;

public class KeyboardInputController : IInputController {
    private Keyboard keyboard;
    private Keyboard Current {
        get {
            if (keyboard == null) {
                keyboard = Keyboard.current;
            }
            return keyboard;
        }
    }

    public float left { get => (Current?.hKey?.isPressed ?? false) ? -1 : 0; }
    public float up { get => (Current?.kKey?.isPressed ?? false) ? 1 : 0; }
    public float right { get => (Current?.lKey?.isPressed ?? false) ? 1 : 0; }
    public float down { get => (Current?.jKey?.isPressed ?? false) ? -1 : 0; }

    public bool jump { get => Current?.zKey?.wasPressedThisFrame ?? false; }

    public bool attack { get => Current?.xKey?.wasPressedThisFrame ?? false; }
}