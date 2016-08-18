using UnityEngine;
using System.Collections;

public class Bone : MonoBehaviour {

    public float gravity;
    private Vector2 velocity;
    public float throwMagnitude;

    private bool direction;
    public bool Direction {
        set {
            direction = value;
        }
    }

    public float lifeTime = 5f;

    void Start () {
        velocity = (Vector2.up + (direction ? Vector2.left : Vector2.right)) * throwMagnitude;
        StartCoroutine(Suicide());
    }

    void Update () {
        velocity.y += gravity * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);
    }

    IEnumerator Suicide() {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
        yield return null;
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
