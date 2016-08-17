using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Skellington : MonoBehaviour {

    public float moveSpeed = 2;

    Controller2D controller;
    Rigidbody2D rigidBody;

    private float gravity = 0f;
    Vector3 velocity;

    public float turnAroundWaitTime = 1f;
    public float turnAroundChance = 0.2f;

    void Start() {
        controller = GetComponent<Controller2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        velocity.x = moveSpeed;
        StartCoroutine(TurnAround());
    }

    void Update() {
        if (gravity == 0f) {
            gravity = GameManager.Instance.PlayerGravity;
        }

        velocity.x = moveSpeed;
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (controller.collisions.right || controller.collisions.rightEdge) {
            moveSpeed = -Mathf.Abs(moveSpeed);
        } 

        if (controller.collisions.left || controller.collisions.leftEdge) {
            moveSpeed = Mathf.Abs(moveSpeed);
        }
    }

    IEnumerator TurnAround() {
        for (;;) {
            yield return new WaitForSeconds(turnAroundWaitTime);
            if (Random.value <= turnAroundChance) {
                moveSpeed *= -1;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == "Whip") {
            Destroy(gameObject);
        }
    }
}
