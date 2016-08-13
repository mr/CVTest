using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Skellington : MonoBehaviour {

    Controller2D controller;
    Rigidbody2D rigidBody;

    float gravity = -1f;
    Vector3 velocity;

    void Start() {
        controller = GetComponent<Controller2D>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update() {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity);
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
