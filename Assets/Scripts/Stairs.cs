using UnityEngine;
using System;

public class Stairs : MonoBehaviour {

    [HideInInspector]
    public new Collider2D collider;

    public static string TAG = "Stairs";

    // Direction stairs are headed (read ltr)
    //  +  == Up,    +  == Down
    // ++            ++
    public enum Grade {
        Up, Down
    }

    public Grade grade;

    [HideInInspector]
    public Vector2 bottom;
    [HideInInspector]
    public Vector2 top;

    // Use this for initialization
    void Start () {
        collider = GetComponent<Collider2D>();
        var min = collider.bounds.min;
        var max = collider.bounds.max;
        switch (grade) {
            case Stairs.Grade.Down:
                bottom = new Vector2(max.x, min.y);
                top = new Vector2(min.x, max.y);
                break;
            case Stairs.Grade.Up:
            default:
                bottom = new Vector2(min.x, min.y);
                top = new Vector2(max.x, max.y);
                break;
        }
    }

    // Update is called once per frame
    void Update () {

    }

    // Is the player trying to move positively on the y axis?
    // Check y axis first since that is prioritized when moving
    public bool IsHeadingUp(Vector2 direction) {
        if (direction.y > 0) {
            return true;
        } else if (direction.y < 0) {
            return false;
        }

        return (direction.x > 0 && grade == Stairs.Grade.Up) || (direction.x < 0 && grade == Stairs.Grade.Down);
    }

    // If we're near the bottom make sure we're moving up
    // if we're near the top make sure we're going down
    // == is !xor for booleans
    // Make sure the player foot contains the point we want to lerp to
    public bool allowedToStartClimb(Vector2 position, Bounds bounds, Vector2 direction) {
        if (direction.y == 0) {
            return false;
        }

        bool isHeadingUp = IsHeadingUp(direction);
        if (nearBottom(position) != isHeadingUp) {
            return false;
        }

        var topOrBottom = isHeadingUp ? bottom : top;
        return bounds.Contains(topOrBottom);
    }

    public bool nearBottom(Vector2 position) {
        var topDistance = Vector2.Distance(position, top);
        var bottomDistance = Vector2.Distance(position, bottom);
        return bottomDistance < topDistance;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, grade == Grade.Up ? 45 : -45), new Vector3(1, 0.25f, 1));
        Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));
    }
}
