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
        Horizontal, Vertical, None
    }

    public FollowMode followMode = FollowMode.Horizontal;

    private bool movedBefore = false;

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
        var hits =
            from hit in Physics2D.RaycastAll(
                origin, direction * targetDistanceSign, Mathf.Infinity, collisionMask)
            where hit.collider.gameObject.CompareTag(
                followMode == FollowMode.Horizontal
                    ? MODE_HORIZONTAL_TAG
                    : MODE_VERITCAL_TAG)
            select hit;
        var firstHit = hits.FirstOrDefault();

        bool hitBarrier = false;
        var velocity = direction * targetDistance;
        if (firstHit) {
            var hitDistance = firstHit.distance * velocity.normalized;
            if (hitDistance.magnitude < velocity.magnitude) {
                velocity = hitDistance;
                hitBarrier = true;
            }
        }

        if (hitBarrier) {
            if (movedBefore) {
                if (followMode == FollowMode.Horizontal) {
                    followMode = FollowMode.Vertical;
                } else if (followMode == FollowMode.Vertical) {
                    followMode = FollowMode.Horizontal;
                }
                movedBefore = false;
            }
            if (velocity.magnitude != 0) {
                movedBefore = true;
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
