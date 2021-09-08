using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour, IEnemy {
    public int damage = 1;
    public float frequency = 5;
    public float amplitude = 5;
    public float speed = 5;
    private float center;

    private float time = 0;

    private bool sleeping = true;
    private float? swoopAmplitude;
    private float direction = 1;

    private Player player;

    void Start() {
        player = Player.GetInstance();
    }

    void Update() {
        var playerPos = player.transform.position;
        var pos = transform.position;
        var distanceX = Mathf.Abs(playerPos.x - pos.x);
        var distanceY = Mathf.Abs(playerPos.y - pos.y);

        Debug.Log("Distance X: " + distanceX);
        Debug.Log("Distance Y: " + distanceY);

        if (sleeping && distanceY > 2) {
            return;
        }

        if (sleeping && distanceX > 6) {
            return;
        }

        if (sleeping) {
            sleeping = false;
            swoopAmplitude = distanceY;
            center = player.Bounds.center.y;
            direction = Mathf.Sign(playerPos.x - pos.x);
        }

        var newSwoopYPos =
            swoopAmplitude != null
                ? swoopAmplitude.Value * Mathf.Cos(frequency * time) + center
                : (float?) null;

        if (newSwoopYPos != null && newSwoopYPos.Value <= center) {
            swoopAmplitude = null;
            newSwoopYPos = null;
            time = 0;
        }

        var newYPos =
            newSwoopYPos != null
                ? newSwoopYPos.Value
                : -amplitude * Mathf.Sin(frequency * time) + center;

        transform.position = new Vector3(
            direction * speed * Time.deltaTime + transform.position.x,
            newYPos,
            transform.position.z);

        time += Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Whip.TAG) {
            Destroy(gameObject);
        }
    }

    public int GetDamage() {
        return damage;
    }

    private readonly struct SwoopState {
        public readonly float amplitude;

        public SwoopState(float amplitude) {
            this.amplitude = amplitude;
        }
    }
}
