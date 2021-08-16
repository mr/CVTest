using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CVCameraFollow : MonoBehaviour {

    public Controller2D target;

    public LayerMask collisionMask;

    public SceneLoader.SceneLoader sceneLoader;

    new Camera camera;

    public enum FollowMode {
        Horizontal, Vertical, None
    }

    public FollowMode followMode = FollowMode.Horizontal;

    void Start() {
        camera = GetComponent<Camera>();
    }

    void LateUpdate() {
        if (!sceneLoader.ready) {
            return;
        }

        var direction = Vector2.zero;
        var targetDistance = 0f;
        if (followMode == FollowMode.Horizontal) {
            direction = Vector2.right;
            targetDistance = target.transform.position.x - transform.position.x ;
        } else if (followMode == FollowMode.Vertical) {
            direction = Vector2.up;
            targetDistance = target.transform.position.y - transform.position.y ;
        }

        var targetDistanceSign = Mathf.Sign(targetDistance);
        var bounds = OrthographicBounds(camera);
        var origin = targetDistanceSign == -1 ? bounds.min : bounds.max;
        var hit = Physics2D.Raycast(origin, direction * targetDistanceSign, Mathf.Infinity, collisionMask);

        var velocity = direction * targetDistance;
        if (hit) {
            var hitDistance = hit.distance * velocity.normalized;
            if (hitDistance.magnitude < velocity.magnitude) {
                velocity = hitDistance;
            }
        }

        transform.Translate(velocity);
    }

    public static Bounds OrthographicBounds(Camera camera) {
         var screenAspect = (float)Screen.width / (float)Screen.height;
         var cameraHeight = camera.orthographicSize * 2;
         Bounds bounds = new Bounds(
             camera.transform.position,
             new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
         return bounds;
     }

}
