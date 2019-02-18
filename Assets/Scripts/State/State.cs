using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : ScriptableObject {
    public virtual void StartState(StateController stateController) {

    }
    public abstract void UpdateState(StateController stateController);
}
