
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CV/States/WhipWindUp")]
public class WhipWindUpState : CoroutineState {
    public WhipOutState whipOutState;
    public override IEnumerator Coroutine(StateController stateController) {
        Whip whip = (Whip) stateController.component;
        yield return new WaitForSeconds(whip.timeToStartWhip);
        stateController.ChangeState(whipOutState);
        yield return null;
    }
}