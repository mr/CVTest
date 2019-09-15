using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour, IEnemy {
    public int damage = 1;
    public float frequency = 5;
    public float amplitude = 5;
    public float speed = 5;
    private float center;

    void Start() {
        center = transform.position.y;
    }

    void Update() {
        transform.position = new Vector3(
            speed * Time.deltaTime + transform.position.x,
            Mathf.Sin(frequency * Time.time) * amplitude + center,
            transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Whip.TAG) {
            Destroy(gameObject);
        }
    }

    public int GetDamage() {
        return damage;
    }
}
