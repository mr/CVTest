using UnityEngine;

namespace SceneLoader {
public class SceneTrigger : MonoBehaviour {
    public string entrance;
    public string exit;
    public Orientation orientation;
    public enum Orientation {
        Horizontal, Vertical
    }

    private new Collider2D collider;

    private void Start() {
        collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Collision with SceneTrigger");
        if (!other.gameObject.CompareTag(Player.Tag)) {
            return;
        }

        var bounds = collider.bounds;
        var min = bounds.min;
        var max = bounds.max;
        var topLeft = new Vector2(min.x, max.y);
        var bottomLeft = new Vector2(min.x, min.y);
        var topRight = new Vector2(max.x, max.y);
        var player = other.gameObject.GetComponent<Player>();
        SceneLoadRequest? request = null;
        if (orientation == Orientation.Horizontal) {
            if (other.bounds.IntersectRay(new Ray(topLeft, Vector2.down))) {
                // Left (entrance)
                request = new SceneLoadRequest(entrance, exit);
            } else if (other.bounds.IntersectRay(new Ray(topRight, Vector2.down))) {
                // Right (exit)
                request = new SceneLoadRequest(exit, entrance);
            }
        } else {
            if (other.bounds.IntersectRay(new Ray(topLeft, Vector2.right))) {
                // Top (entrance)
                request = new SceneLoadRequest(entrance, exit);
            } else if (other.bounds.IntersectRay(new Ray(bottomLeft, Vector2.right))) {
                // Bottom (exit)
                request = new SceneLoadRequest(exit, entrance);
            }
        }

        if (request != null) {
            player.sceneLoader.QueueLoad(request.Value);
        }
    }

    private void OnDrawGizmos() {
        var bounds = new Bounds(transform.position, transform.localScale);
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
