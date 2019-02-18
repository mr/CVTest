using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CV/States/WhipOut")]
public class WhipOutState : CoroutineState {
    // Start is called before the first frame update
    public WhipRestState whipRestState;
    public override IEnumerator Coroutine(StateController stateController) {
        Whip whip = (Whip) stateController.component;
        whip.WhipOut = true;
        yield return new WaitForSeconds(whip.timeToWhip);
        stateController.ChangeState(whipRestState);
        yield return null;
    }
}
