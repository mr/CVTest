using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CV/States/WhipRestState")]
public class WhipRestState : State {
    public WhipWindUpState whipWindUpState;
    public override void StartState(StateController stateController) {
        Whip whip = (Whip) stateController.component;
        whip.WhipOut = false;
    }

    public override void UpdateState(StateController stateController) {
        if (Input.GetKeyDown(KeyCode.Z)) {
            stateController.ChangeState(whipWindUpState);
        }
    }
}
