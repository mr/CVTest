using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Skellington : MonoBehaviour, IEnemy {

    public float moveSpeed = 2;
    public int damage = 1;

    public Bone bone;

    Controller2D controller;
    private float gravity = 0f;
    Vector3 velocity;

    public bool turnsAroundRandomly;
    public float turnAroundWaitTime = 1f;
    public float turnAroundChance = 0.2f;

    public bool throwsBones;
    public float boneThrowWaitTime = 1f;
    public float boneThrowChance = 0.2f;
    public float boneThrowPauseTime = 0.2f;

    public int boneThrowMaxWaitTime = 5;

    private bool throwing;
    private Player player;

    void Awake() {
        player = Player.Instance;
    }

    void Start() {
        controller = GetComponent<Controller2D>();
        velocity.x = moveSpeed;
        if (turnsAroundRandomly) {
            StartCoroutine(TurnAround());
        }
        if (throwsBones) {
            StartCoroutine(ThrowBones());
        }
    }

    void Update() {
        if (gravity == 0f) {
            gravity = Player.Instance.Gravity;
        }

        if (throwing || Vector2.Distance(player.transform.position, transform.position) > 20) {
            velocity.x = 0;
        } else {
            velocity.x = moveSpeed;
        }
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

    IEnumerator ThrowBones() {
        int timesWaited = 0;
        for (;;) {
            yield return new WaitForSeconds(boneThrowWaitTime);
            if (Random.value <= boneThrowChance || timesWaited == boneThrowMaxWaitTime) {
                timesWaited = 0;
                throwing = true;
                yield return new WaitForSeconds(boneThrowPauseTime);
                if (Vector2.Distance(transform.position, player.transform.position) < 20) {
                    Bone.Create(
                        Util.DirectionOf(of: player.gameObject, relativeTo: gameObject),
                        transform.position,
                        transform.rotation
                    );
                    throwing = false;
                }
            } else {
                timesWaited++;
            }
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
        if (collider.gameObject.tag == Whip.TAG) {
            Destroy(gameObject);
        }
    }

    public int GetDamage() {
        return damage;
    }
}
