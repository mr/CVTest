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

    // Is the player trying to move positively on the y axis?
    public bool isHeadingUp(Vector2 direction) =>
        (direction.x > 0 && grade == Stairs.Grade.Up) || (direction.x < 0 && grade == Stairs.Grade.Down) || (direction.y > 0);

    // If we're near the bottom make sure we're moving up
    // if we're near the top make sure we're going down
    // == is !xor for booleans
    public bool allowedToStartClimb(Vector2 position, Vector2 direction) =>
        (direction.y != 0) && (nearBottom(position) == isHeadingUp(direction));

    public bool nearBottom(Vector2 position) {
        var topDistance = Vector2.Distance(position, top);
        var bottomDistance = Vector2.Distance(position, bottom);
        return bottomDistance < topDistance;
    }
}
