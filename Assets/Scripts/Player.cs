using UnityEngine;

using Enums;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 6;

    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    bool wasOnGround = false;
    bool jumped = false;
    bool falling = false;
    bool climbingStairs = false;

    int numOnStairs = 0;
    bool onStairs {
        get {
            return numOnStairs > 0;
        }

        set {
            if (value) {
                numOnStairs++;
            } else {
                numOnStairs--;
            }
        }
    }
    Stairs.Grade stairGrade;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    public Vector2 DirectionalInput {
        set {
            if (value.x < 0) {
                whip.WhipDirection = Direction.Left;
            } else if (value.x > 0) {
                whip.WhipDirection = Direction.Right;
            }

            if (controller.collisions.below || onStairs) {
                directionalInput = value;
            }
        }
        get { return directionalInput; }
    }

    Vector2 stairDirectionalInput;
    public Vector2 StairDirectionalInput {
        set {
            stairDirectionalInput = value;
        }
    }

    bool wallSliding;
    int wallDirX;

    public Whip whip;

    void Start() {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        GameManager.Instance.PlayerGravity = gravity;
    }

    void EarlyUpdate() {
        if (controller.collisions.below && velocity.y <= 0) {
            jumped = false;
            falling = false;
        }
        climbingStairs = onStairs && stairDirectionalInput.x != 0;
        CalculateVelocity();
    }

    void Update() {
        EarlyUpdate();

        if (climbingStairs) {
            Debug.Log("On Stairs");
            velocity.x = velocity.y = stairDirectionalInput.x * moveSpeed;
            if (stairGrade == Stairs.Grade.Down) {
                velocity.y *= -1;
            }
        } else if (onStairs) {
            velocity.y = 0;
        }

        if (falling) {
            velocity.x = 0;
        }

        if (whip.whipping && controller.collisions.below) {
            velocity.x = 0;
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (!jumped && wasOnGround && !controller.collisions.below && !onStairs) {
            falling = true;
        }
    }

    void LateUpdate() {
        wasOnGround = controller.collisions.below;
    }

    public void OnJumpInputDown() {
        if (wallSliding) {
            if (wallDirX == directionalInput.x) {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            } else if (directionalInput.x == 0) {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            } else {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below) {
            velocity.y = maxJumpVelocity;
            jumped = true;
        }
    }

    public void OnJumpInputUp() {
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    void HandleWallSliding() {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax) {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0) {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0) {
                    timeToWallUnstick -= Time.deltaTime;
                } else {
                    timeToWallUnstick = wallStickTime;
                }
            } else {
                timeToWallUnstick = wallStickTime;
            }

        }

    }

    void CalculateVelocity() {
        velocity.x = directionalInput.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            Debug.Log("Stairs Enter");
            stairGrade = collider.gameObject.GetComponent<Stairs>().grade;
            onStairs = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            Debug.Log("Stairs Exit");
            onStairs = false;
        }
    }
}
