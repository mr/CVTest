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
    }

    void addStairs() {
        numOnStairs++;
    }

    void removeStairs() {
        numOnStairs--;
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

    Collider2D collider;

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

    StairLocation? stairStartLocation = null;
    struct StairLocation {
        public Vector2 bottom;
        public Vector2 top;

        public StairLocation(Collider2D collider, Stairs stairs) {
            var min = collider.bounds.min;
            var max = collider.bounds.max;
            switch (stairs.grade) {
                case Stairs.Grade.Down:
                    bottom = new Vector2(max.x, min.y);
                    top = new Vector2(min.x, max.y);
                    break;
                case Stairs.Grade.Up:
                default:
                    bottom = new Vector2(min.x, min.y);
                    top = new Vector2(max.x, max.y);
                    break;
            }
        }
    }

    StairLerpState? stairLerpState = null;
    struct StairLerpState {
        public float startTime;
        public float length;
        public Vector2 start;
        public Vector2 end;

        public StairLerpState(Vector2 start, Vector2 end) {
            startTime = Time.time;
            this.length = Vector2.Distance(start, end);
            this.start = start;
            this.end = end;
        }
    }

    int wallDirX;

    public Whip whip;

    void Start() {
        controller = GetComponent<Controller2D>();
        collider = GetComponent<Collider2D>();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        GameManager.Instance.PlayerGravity = gravity;
    }

    void Update() {
        GetInput();

        if (controller.collisions.below && velocity.y <= 0) {
            jumped = false;
            falling = false;
        }

        if (!climbingStairs) {
            climbingStairs = onStairs && stairDirectionalInput.x != 0 && controller.collisions.below;
            // climbing stairs for the first time
            if (climbingStairs) {
                if ((stairDirectionalInput.x > 0 && stairGrade == Stairs.Grade.Up) || (stairDirectionalInput.x < 0 && stairGrade == Stairs.Grade.Down)) {
                    stairLerpState = new StairLerpState(getPlayerBottom(), stairStartLocation.Value.bottom);
                } else {
                    stairLerpState = new StairLerpState(getPlayerBottom(), stairStartLocation.Value.top);
                }
            }
        }
        CalculateVelocity();

        if (climbingStairs) {
            Debug.Log("On Stairs");
            if (stairLerpState != null) {
                if (stairDirectionalInput.x != 0) {
                    var startTime = stairLerpState.Value.startTime;
                    var journeyLength = stairLerpState.Value.length;
                    var start = stairLerpState.Value.start;
                    var end = stairLerpState.Value.end;
                    var distCovered = (Time.time - startTime) * moveSpeed;
                    var fracJourney = distCovered / journeyLength;
                    if (getPlayerBottom() == end) {
                        var newBottomPos = Vector2.Lerp(start, end, fracJourney);
                        var oldPos = transform.position;
                        transform.position = new Vector3(newBottomPos.x, newBottomPos.y + collider.bounds.extents.y, oldPos.z);
                    } else {
                        stairLerpState = null;
                    }
                }
            } else {
                velocity.x = velocity.y = stairDirectionalInput.x * moveSpeed;
                if (stairGrade == Stairs.Grade.Down) {
                    velocity.y *= -1;
                }
            }
        }

        if (falling) {
            velocity.x = 0;
        }

        // on ground and whipping
        if (whip.whipping && controller.collisions.below) {
            velocity.x = 0;
        }

        controller.Move(velocity * Time.deltaTime, directionalInput, climbingStairs: climbingStairs);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (!jumped && wasOnGround && !controller.collisions.below && !onStairs) {
            falling = true;
        }

        if (climbingStairs && !onStairs) {
            climbingStairs = false;
        }
    }

    void LateUpdate() {
        wasOnGround = controller.collisions.below;
    }

    void GetInput() {
        DirectionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.A)) {
            StairDirectionalInput = Vector2.left;
        } else if (Input.GetKey(KeyCode.S)) {
            StairDirectionalInput = Vector2.right;
        } else {
            StairDirectionalInput = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            OnJumpInputUp();
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            whip.DoWhip();
        }
    }

    public void OnJumpInputDown() {
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

    void CalculateVelocity() {
        velocity.x = directionalInput.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            Debug.Log("Stairs Enter");
            var stairs = collider.gameObject.GetComponent<Stairs>();
            stairGrade = stairs.grade;
            // ready to climb stairs for the first time
            if (!onStairs && controller.collisions.below && stairs.end) {
                stairStartLocation = new StairLocation(collider, stairs);
            }
            addStairs();
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            Debug.Log("Stairs Exit");
            removeStairs();
            if (!onStairs) {
                stairStartLocation = null;
            }
        }
    }

    Vector2 getPlayerBottom() {
        return new Vector2(collider.bounds.center.x, collider.bounds.min.y);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (stairStartLocation != null) {
            var loc = stairStartLocation.Value;
            Gizmos.DrawSphere(new Vector3(loc.bottom.x, loc.bottom.y, 0), 0.2f);
            Gizmos.DrawSphere(new Vector3(loc.top.x, loc.top.y, 0), 0.2f);
        }
    }
}
