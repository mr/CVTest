using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
public class Whip : MonoBehaviour {

    BoxCollider2D boxCollider;
    MeshRenderer meshRenderer;

    public enum Direction {
        Left, Right
    }

    public Direction WhipDirection {
        set {
            var x = transform.localPosition.x;
            var y = transform.localPosition.y;
            switch (value) {
                case Direction.Left:
                    transform.localPosition = new Vector3(-Math.Abs(x), y, 0);
                    break;
                case Direction.Right:
                    transform.localPosition = new Vector3(Math.Abs(x), y, 0);
                    break;
            }
        }
    }

    // Use this for initialization
    void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            boxCollider.enabled = !boxCollider.enabled;
            meshRenderer.enabled = !meshRenderer.enabled;
        }

        Debug.Log(transform.position);
    }
}
