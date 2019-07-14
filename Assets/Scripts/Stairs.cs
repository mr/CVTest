using UnityEngine;
using System;

public class Stairs : MonoBehaviour {

    [HideInInspector]
    public new Collider2D collider;

    public static string TAG = "Stairs";

    public enum Grade {
        Up, Down
    }

    public bool end = false;

    public Grade grade;

    public Vector2 bottom;
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

    public bool directionUpOrDown(Vector2 direction) =>
        (direction.x > 0 && grade == Stairs.Grade.Up) || (direction.x < 0 && grade == Stairs.Grade.Down);

    // If we're near the bottom make sure we're moving up
    // if we're near the top make sure we're going down
    // == is !xor for booleans
    public bool allowedToClimb(Vector2 position, Vector2 direction) =>
        nearBottom(position) == directionUpOrDown(direction);

    public bool nearBottom(Vector2 position) {
        var topDistance = Vector2.Distance(position, top);
        var bottomDistance = Vector2.Distance(position, bottom);
        return bottomDistance < topDistance;
    }
}
