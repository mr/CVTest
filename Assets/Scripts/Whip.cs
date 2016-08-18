﻿using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
public class Whip : MonoBehaviour {

    BoxCollider2D boxCollider;
    MeshRenderer meshRenderer;

    public float timeToStartWhip = 0.2f;
    public float timeToWhip = 0.3f;

    public enum Direction {
        Left, Right
    }

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

    bool whipping = false;
    public bool Whipping {
        protected set {
            whipping = value;
        }

        get {
            return whipping;
        }
    }

    bool whipOut = false;
    bool WhipOut {
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
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (!Whipping) {
                StartCoroutine("WhipInTime");
            }
        }
    }

    IEnumerator WhipInTime() {
        Whipping = true;

        yield return new WaitForSeconds(timeToStartWhip);

        WhipOut = true;

        yield return new WaitForSeconds(timeToWhip);

        Whipping = false;
        WhipOut = false;

        yield return null;
    }
}
