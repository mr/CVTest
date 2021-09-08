public interface IInputController {
    float left { get; }
    float up { get; }
    float right { get; }
    float down { get; }

    bool jump { get; }

    bool attack { get; }
}