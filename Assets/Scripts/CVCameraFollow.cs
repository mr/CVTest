using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class CVCameraFollow : MonoBehaviour {

    public static string MODE_HORIZONTAL_TAG = "Horizontal";
    public static string MODE_VERITCAL_TAG = "Vertical";

    public Controller2D target;

    public LayerMask collisionMask;

    public SceneLoader.SceneLoader sceneLoader;

    new Camera camera;

    public enum FollowMode {
        Horizontal, Vertical
    }

    public FollowMode followMode = FollowMode.Horizontal;

    private bool stopped = false;

    void Start() {
        camera = GetComponent<Camera>();
    }

    void LateUpdate() {
        if (!sceneLoader.ready || stopped) {
            return;
        }

        var bounds = OrthographicBounds(camera);

        var direction = Vector2.zero;
        var targetDistance = 0f;
        var origin = Vector2.zero;
        var targetDistanceSign = 1f;
        if (followMode == FollowMode.Horizontal) {
            direction = Vector2.right;
            targetDistance = target.transform.position.x - transform.position.x ;
            targetDistanceSign = Mathf.Sign(targetDistance);
            origin =
                targetDistanceSign == -1
                    ? new Vector2(
                        bounds.center.x - bounds.extents.x, bounds.center.y)
                    : new Vector2(
                        bounds.center.x + bounds.extents.x, bounds.center.y);
        } else if (followMode == FollowMode.Vertical) {
            direction = Vector2.up;
            targetDistance = target.transform.position.y - transform.position.y ;
            targetDistanceSign = Mathf.Sign(targetDistance);
            origin =
                targetDistanceSign == -1
                    ? new Vector2(
                        bounds.center.x, bounds.center.y - bounds.extents.y)
                    : new Vector2(
                        bounds.center.x, bounds.center.y + bounds.extents.y);
        }

        var hits =
            from hit in Physics2D.RaycastAll(
                origin, direction * targetDistanceSign, Mathf.Infinity, collisionMask)
            where hit.collider.gameObject.CompareTag(
                followMode == FollowMode.Horizontal
                    ? MODE_HORIZONTAL_TAG
                    : MODE_VERITCAL_TAG)
            select hit;
        var firstHit = hits.FirstOrDefault();

        var velocity = direction * targetDistance;
        if (firstHit) {
            if (firstHit.distance < velocity.magnitude) {
                velocity = firstHit.distance * velocity.normalized;
                var barrier = firstHit.collider.gameObject.GetComponent<CameraBarrier>();
                if (barrier.switcher) {
                    if (followMode == FollowMode.Horizontal) {
                        followMode = FollowMode.Vertical;
                    } else {
                        followMode = FollowMode.Horizontal;
                    }
                }
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
