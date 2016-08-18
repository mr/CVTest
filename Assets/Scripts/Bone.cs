using UnityEngine;

public class Bone : MonoBehaviour {

    public float gravity;
    private Vector2 velocity;
    public float throwMagnitude;

    void Start () {
        velocity = (Vector2.up + Vector2.left) * throwMagnitude;
    }

    void Update () {
        velocity.y += gravity * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Debug.Log("Trigger");
        if (collider.gameObject.tag == "Player") {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Collision");
        if (collision.gameObject.tag == "Player") {
            Destroy(gameObject);
        }
    }
}
