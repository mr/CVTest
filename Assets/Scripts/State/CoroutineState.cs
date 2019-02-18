using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CoroutineState : State {
    private HashSet<int> startedSet = new HashSet<int>();
    public override sealed void UpdateState(StateController stateController) {
        if (!startedSet.Contains(stateController.component.GetInstanceID())) {
            stateController.component.StartCoroutine(CoroutineWrapper(stateController));
        }
        startedSet.Add(stateController.component.GetInstanceID());
    }

    private IEnumerator CoroutineWrapper(StateController stateController) {
        yield return Coroutine(stateController);
        startedSet.Remove(stateController.component.GetInstanceID());
        yield return null;
    }

    public abstract IEnumerator Coroutine(StateController stateController);
}
