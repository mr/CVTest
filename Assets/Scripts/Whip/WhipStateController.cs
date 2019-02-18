using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Whip))]
public class WhipStateController : StateController {
    protected override void Start() {
        component = GetComponent<Whip>();
        base.Start();
    }
}
