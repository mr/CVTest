using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateController : MonoBehaviour {
    public MonoBehaviour component;
    public State currentState;
    // Start is called before the first frame update
    protected virtual void Start() {
        currentState.StartState(this);
    }

    protected virtual void Update() {
        currentState.UpdateState(this);
    }

    public void ChangeState(State newState) {
        currentState = newState;
        currentState.StartState(this);
    }
}
