using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class SceneTrigger : MonoBehaviour {
    public string entrance;
    public string exit;
    public Orientation orientation;
    public enum Orientation {
        Horizontal, Vertical
    }

    new Collider2D collider;

    void Start() {
        collider = GetComponent<Collider2D>();
    }

    void Update() {

    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == Player.Tag) {
            var min = collider.bounds.min;
            var max = collider.bounds.max;
            var topLeft = new Vector2(min.x, max.y);
            var bottomLeft = new Vector2(min.x, min.y);
            var topRight = new Vector2(max.x, max.y);
            var player = other.gameObject.GetComponent<Player>();
            if (orientation == Orientation.Horizontal) {
                if (other.bounds.IntersectRay(new Ray(topLeft, Vector2.down))) {
                    // Left (entrance)
                    player.sceneLoader.QueueLoad(new SceneLoadRequest(entrance, exit));
                } else if (other.bounds.IntersectRay(new Ray(topRight, Vector2.down))) {
                    // Right (exit)
                    player.sceneLoader.QueueLoad(new SceneLoadRequest(exit, entrance));
                }
            } else {
                if (other.bounds.IntersectRay(new Ray(topLeft, Vector2.right))) {
                    // Top (entrance)
                    player.sceneLoader.QueueLoad(new SceneLoadRequest(entrance, exit));
                } else if (other.bounds.IntersectRay(new Ray(bottomLeft, Vector2.right))) {
                    // Bottom (exit)
                    player.sceneLoader.QueueLoad(new SceneLoadRequest(exit, entrance));
                }
            }
        }
    }

    void OnDrawGizmos() {
        if (collider == null) {
            return;
        }

        var min = collider.bounds.min;
        var max = collider.bounds.max;
        if (orientation == Orientation.Horizontal) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector2(min.x, max.y), new Vector2(min.x, min.y));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(max.x, max.y), new Vector2(max.x, min.y));
        } else {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector2(min.x, max.y), new Vector2(max.x, max.y));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(min.x, min.y), new Vector2(max.x, min.y));
        }
    }
}
