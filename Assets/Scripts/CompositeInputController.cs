using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CompositeInputController : IInputController {
    private List<IInputController> controllers;

    public float left { get => activeController.left; }
    public float up { get => activeController.up; }
    public float right { get => activeController.right; }
    public float down { get => activeController.down; }

    public bool jump { get => activeController.jump; }

    public bool attack { get => activeController.attack; }

    private IInputController activeController {
        get {
            foreach (var controller in controllers) {
                if (controller.left != 0
                    || controller.up != 0
                    || controller.right != 0
                    || controller.down != 0
                    || controller.jump
                    || controller.attack) {
                    return controller;
                }
            }
            return controllers[0];
        }
    }

    public CompositeInputController(params IInputController[] controllers) {
        this.controllers = new List<IInputController>(controllers);
    }
}