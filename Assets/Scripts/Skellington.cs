using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Skellington : MonoBehaviour {

    public float moveSpeed = 2;

    Controller2D controller;
    Rigidbody2D rigidBody;

    public float gravity = -1f;
    Vector3 velocity;

    float travelDistance = 0;

    void Start() {
        controller = GetComponent<Controller2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        velocity.x = moveSpeed;
    }

    void Update() {
        velocity.y += gravity * Time.deltaTime;

        var startX = transform.localPosition.x;
        controller.Move(velocity * Time.deltaTime);
        var endX = transform.localPosition.x;

        travelDistance += Mathf.Abs(startX - endX);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.name == "Whip") {
            Debug.Log("Suicide");
            Destroy(gameObject);
        }
    }
}
