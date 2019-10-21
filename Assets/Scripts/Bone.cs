using UnityEngine;
using System.Collections;

using Enums;

public class Bone : MonoBehaviour, IEnemy {

    public float gravity;
    private Vector2 velocity;
    public float throwMagnitude;
    public int damage = 1;

    public static GameObject Create(Direction direction, Vector2 position, Quaternion rotation) {
        var prefab = Resources.Load("Prefabs/Bone");
        var gameObject = Instantiate(prefab, position, rotation) as GameObject;
        var newBone = gameObject.GetComponent<Bone>();
        newBone.Direction = direction;
        return gameObject;
    }

    private Direction direction;
    public Direction Direction {
        set {
            direction = value;
        }
    }

    public float lifeTime = 5f;

    void Start () {
        var throwDir = Util.DirectionToVector2(direction);
        velocity = (Vector2.up + throwDir) * throwMagnitude;
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
        if (collider.gameObject.tag == "Player") {
            Destroy(gameObject);
        }
    }

    public int GetDamage() {
        return damage;
    }
}
