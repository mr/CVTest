using UnityEngine;
using Enums;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    public LayerMask stairMask;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 6;
    public float playerFootSize = 0.3f;

    bool wasOnGround = false;
    bool jumped = false;
    bool falling = false;
    bool climbingStairs = false;

    int numOnStairs = 0;
    bool touchingStairs {
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

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    new Collider2D collider;

    Vector2 directionalInput;
    public Vector2 DirectionalInput {
        set {
            if (value.x < 0) {
                whip.WhipDirection = Direction.Left;
            } else if (value.x > 0) {
                whip.WhipDirection = Direction.Right;
            }

            if (controller.collisions.below || touchingStairs) {
                directionalInput = value;
            }
        }
        get { return directionalInput; }
    }

    // Vector2 stairDirectionalInput;

    Stairs validStair = null;
    HashSet<Stairs> stairEnds = new HashSet<Stairs>();

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
            validStair = stairEnds
                .Where(stair =>
                    stair.collider.bounds.Intersects(getPlayerBottomBounds())
                        && stair.allowedToStartClimb(getPlayerBottom(), DirectionalInput))
                .FirstOrDefault();
            climbingStairs = touchingStairs
                && validStair != null
                && controller.collisions.below;
            // climbing stairs for the first time
            if (climbingStairs) {
                var topOrBottom = validStair.isHeadingUp(DirectionalInput) ? validStair.bottom : validStair.top;
                stairLerpState = new StairLerpState(getPlayerBottom(), topOrBottom);
            }
        }
        CalculateVelocity();

        if (climbingStairs) {
            // Default to not moving on the stairs (aka don't fall through the stairs)
            velocity = Vector2.zero;
            if (stairLerpState != null) {
                if (DirectionalInput != Vector2.zero) {
                    var startTime = stairLerpState.Value.startTime;
                    var journeyLength = stairLerpState.Value.length;
                    var start = stairLerpState.Value.start;
                    var end = stairLerpState.Value.end;
                    var distCovered = (Time.time - startTime) * moveSpeed;
                    var fracJourney = distCovered / journeyLength;
                    if (getPlayerBottom() != end) {
                        var newBottomPos = Vector2.Lerp(start, end, fracJourney);
                        setPlayerBottom(newBottomPos);
                    } else {
                        // finished lerping to start of stairs
                        stairLerpState = null;
                    }
                }
            } else {
                var isYInput = DirectionalInput.y != 0;
                velocity.x = velocity.y =  (isYInput ? DirectionalInput.y : DirectionalInput.x) * moveSpeed;
                if (validStair != null && validStair.grade == Stairs.Grade.Down) {
                    if (isYInput) {
                        velocity.x *= -1;
                    } else {
                        velocity.y *= -1;
                    }
                }
                if (willLeaveStairs(velocity * Time.deltaTime)) {
                    velocity = Vector2.zero;
                    Stairs currentStairs = getEndStairsPlayerIsOn();
                    var topOrBottom = currentStairs.isHeadingUp(DirectionalInput) ? currentStairs.top : currentStairs.bottom;
                    transform.position = new Vector3(topOrBottom.x, topOrBottom.y + collider.bounds.extents.y, 0);
                    climbingStairs = false;
                    validStair = null;
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

        controller.Move(velocity * Time.deltaTime, DirectionalInput, climbingStairs: climbingStairs);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (!jumped && wasOnGround && !controller.collisions.below && !touchingStairs) {
            falling = true;
        }

        if (climbingStairs && !touchingStairs) {
            climbingStairs = false;
        }
    }

    void LateUpdate() {
        wasOnGround = controller.collisions.below;
    }

    void GetInput() {
        DirectionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // DirectionalInput = new Vector2(1, -1);

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
        velocity.x = DirectionalInput.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
    }

    bool willLeaveStairs(Vector3 newVelocity) {
        Stairs stairs = getEndStairsPlayerIsOn();
        Vector2 newVelocity2 = new Vector2(newVelocity.x, newVelocity.y);
        var collidedStairs = Physics2D.OverlapPoint(getPlayerBottom() + newVelocity2, layerMask: stairMask);
        return collidedStairs == null;
    }

    Vector2 getPlayerBottom() {
        return new Vector2(collider.bounds.center.x, collider.bounds.min.y);
    }

    void setPlayerBottom(Vector2 newPos) {
        var oldPos = transform.position;
        transform.position = new Vector3(newPos.x, newPos.y + collider.bounds.extents.y, oldPos.z);
    }

    Bounds getPlayerBottomBounds() {
        return new Bounds(getPlayerBottom(), new Vector2(collider.bounds.size.x, playerFootSize));
    }

    Stairs getEndStairsPlayerIsOn() {
        return stairEnds
            .Where(stairEnd => stairEnd.collider.bounds.Contains(getPlayerBottom()))
            .FirstOrDefault();
    }
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            var stairs = collider.gameObject.GetComponent<Stairs>();
            // ready to climb stairs for the first time
            if (controller.collisions.below && stairs.end) {
                stairEnds.Add(stairs);
            }

            if (touchingStairs && stairs.end) {
                stairEnds.Add(stairs);
            }
            addStairs();
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            removeStairs();
            if (!touchingStairs) {
                stairEnds.Clear();
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (validStair != null) {
            Gizmos.DrawSphere(new Vector3(validStair.bottom.x, validStair.bottom.y, 0), 0.2f);
            Gizmos.DrawSphere(new Vector3(validStair.top.x, validStair.top.y, 0), 0.2f);
        }
        Gizmos.color = Color.green;
        var bottomBounds = getPlayerBottomBounds();
        Gizmos.DrawCube(bottomBounds.center, bottomBounds.size);
    }
}
