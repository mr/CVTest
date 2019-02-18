using UnityEngine;
using System;
using System.Collections;

using Enums;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
public class Whip : MonoBehaviour {

    BoxCollider2D boxCollider;
    MeshRenderer meshRenderer;

    public float timeToStartWhip = 0.2f;
    public float timeToWhip = 0.3f;

    public Direction WhipDirection {
        set {
            var x = Math.Abs(transform.localPosition.x);
            var y = transform.localPosition.y;
            if (value == Direction.Left) {
                x = -x;
            }
            transform.localPosition = new Vector3(x, y, 0);
        }
    }

    [HideInInspector]
    public bool whipping = false;

    bool whipOut = false;
    public bool WhipOut {
        set {
            boxCollider.enabled = value;
            meshRenderer.enabled = value;
            whipOut = value;
        }

        get {
            return whipOut;
        }
    }

    void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    void Update() {
        // if (Input.GetKeyDown(KeyCode.Z)) {
        //     if (!whipping) {
        //         StartCoroutine(WhipInTime());
        //     }
        // }
    }

    IEnumerator WhipInTime() {
        whipping = true;

        yield return new WaitForSeconds(timeToStartWhip);

        WhipOut = true;

        yield return new WaitForSeconds(timeToWhip);

        whipping = false;
        WhipOut = false;

        yield return null;
    }
}
