using UnityEngine;

namespace Scene {
public class SceneTrigger : MonoBehaviour {
    public string entrance;
    public string exit;
    public Orientation orientation;
    public enum Orientation {
        Horizontal, Vertical
    }

    private BoxCollider2D boxCollider;

    private void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag(Player.Tag)) {
            var bounds = boxCollider.bounds;
            var min = bounds.min;
            var max = bounds.max;
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

    private void OnDrawGizmos() {
        if (boxCollider == null) {
            return;
        }

        var bounds = boxCollider.bounds;
        var min = bounds.min;
        var max = bounds.max;
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
}
