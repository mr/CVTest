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

            if (IsOnGround() || touchingStairs) {
                directionalInput = value;
            }
        }
        get { return directionalInput; }
    }

    // Vector2 stairDirectionalInput;

    Stairs validStair = null;
    HashSet<Stairs> stairEnds = new HashSet<Stairs>();

    LerpState? stairLerpState = null;
    struct LerpState {
        public float startTime;
        public float length;
        public Vector2 start;
        public Vector2 end;

        public LerpState(Vector2 start, Vector2 end) {
            startTime = Time.time;
            this.length = Vector2.Distance(start, end);
            this.start = start;
            this.end = end;
        }

        public float FracJourney(float time, float speed) {
            var distCovered = (time - startTime) * speed;
            return distCovered / length;
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

        if (IsOnGround() && velocity.y <= 0) {
            jumped = false;
            falling = false;
        }

        if (!climbingStairs) {
            StartClimbingIfAble();
        }

        CalculateVelocity();

        if (climbingStairs) {
            // Default to not moving on the stairs (aka don't fall through the stairs)
            velocity = Vector2.zero;
            if (stairLerpState != null) {
                if (DirectionalInput != Vector2.zero) {
                    var start = stairLerpState.Value.start;
                    var end = stairLerpState.Value.end;
                    var fracJourney = stairLerpState?.FracJourney(Time.time, moveSpeed);
                    if (GetPlayerBottom() != end) {
                        var newBottomPos = Vector2.Lerp(start, end, fracJourney.Value);
                        SetPlayerBottom(newBottomPos);
                    } else {
                        // finished lerping to start of stairs
                        stairLerpState = null;
                    }
                }
            }

            if (stairLerpState == null) {
                var isYInput = DirectionalInput.y != 0;
                velocity.x = velocity.y =  (isYInput ? DirectionalInput.y : DirectionalInput.x) * moveSpeed;
                if (validStair != null && validStair.grade == Stairs.Grade.Down) {
                    if (isYInput) {
                        velocity.x *= -1;
                    } else {
                        velocity.y *= -1;
                    }
                }
                if (WillLeaveStairs(velocity * Time.deltaTime)) {
                    velocity = Vector2.zero;
                    Stairs currentStairs = GetEndStairsPlayerIsOn();
                    var topOrBottom = currentStairs.IsHeadingUp(DirectionalInput)
                        ? currentStairs.top
                        : currentStairs.bottom;
                    transform.position = new Vector3(topOrBottom.x, topOrBottom.y + collider.bounds.extents.y, 0);
                    StartClimbingIfAble();
                }
            }
        }

        if (falling) {
            velocity.x = 0;
        }

        // on ground and whipping
        if (whip.whipping && IsOnGround()) {
            velocity.x = 0;
        }

        controller.Move(velocity * Time.deltaTime, DirectionalInput, climbingStairs: climbingStairs);

        if (controller.collisions.above || IsOnGround()) {
            velocity.y = 0;
        }

        if (!jumped && wasOnGround && !IsOnGround() && !touchingStairs) {
            falling = true;
        }

        if (climbingStairs && !touchingStairs) {
            climbingStairs = false;
        }
    }

    void LateUpdate() {
        wasOnGround = IsOnGround();
    }

    void GetInput() {
        var axis7 = Input.GetAxisRaw("7th Axis");
        var axis8 = Input.GetAxisRaw("8th Axis");
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        // prioritize dpad :^)
        var xaxis = axis7 != 0 ? axis7 : horizontal;
        var yaxis = axis8 != 0 ? -axis8 : vertical;
        DirectionalInput = new Vector2(xaxis, yaxis);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button1)) {
            OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button1)) {
            OnJumpInputUp();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Joystick1Button0)) {
            whip.DoWhip();
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button7)) {
            Debug.Break();
        }
    }

    public void OnJumpInputDown() {
        if (IsOnGround()) {
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

    void StartClimbingIfAble() {
        validStair = stairEnds
            .Where(stair =>
                stair.collider.bounds.Intersects(GetPlayerBottomBounds())
                    && stair.allowedToStartClimb(GetPlayerBottom(), GetPlayerBottomBounds(), DirectionalInput)
                    && (IsOnGround() || climbingStairs))
            .FirstOrDefault();
        climbingStairs = touchingStairs && validStair != null;
        // climbing stairs for the first time
        if (climbingStairs) {
            var topOrBottom = validStair.IsHeadingUp(DirectionalInput) ? validStair.bottom : validStair.top;
            stairLerpState = new LerpState(GetPlayerBottom(), topOrBottom);
        }
    }

    bool WillLeaveStairs(Vector3 newVelocity) {
        Stairs stairs = GetEndStairsPlayerIsOn();
        Vector2 newVelocity2 = Vector3s.toVector2(newVelocity);
        var collidedStairs = Physics2D.OverlapPoint(GetPlayerBottom() + newVelocity2, layerMask: stairMask);
        return collidedStairs == null;
    }

    Vector2 GetPlayerBottom() {
        return new Vector2(collider.bounds.center.x, collider.bounds.min.y);
    }

    void SetPlayerBottom(Vector2 newPos) {
        var oldPos = transform.position;
        transform.position = new Vector3(newPos.x, newPos.y + collider.bounds.extents.y, oldPos.z);
    }

    Bounds GetPlayerBottomBounds() {
        return new Bounds(GetPlayerBottom(), new Vector2(collider.bounds.size.x, playerFootSize));
    }

    Stairs GetEndStairsPlayerIsOn() {
        return stairEnds
            .Where(stairEnd => stairEnd.collider.bounds.Contains(GetPlayerBottom()))
            .FirstOrDefault();
    }
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            var stairs = collider.gameObject.GetComponent<Stairs>();
            OnStairsTriggerEnter(collider, stairs);
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag == Stairs.TAG) {
            var stairs = collider.gameObject.GetComponent<Stairs>();
            OnStairsTriggerExit(collider, stairs);
        }
    }

    void OnStairsTriggerEnter(Collider2D collider, Stairs stairs) {
        if (stairs.end) {
            stairEnds.Add(stairs);
        }

        addStairs();
    }

    void OnStairsTriggerExit(Collider2D collider, Stairs stairs) {
        removeStairs();
        if (stairs.end) {
            stairEnds.Remove(stairs);
        }

        if (!touchingStairs) {
            stairEnds.Clear();
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (validStair != null) {
            Gizmos.DrawSphere(new Vector3(validStair.bottom.x, validStair.bottom.y, 0), 0.2f);
            Gizmos.DrawSphere(new Vector3(validStair.top.x, validStair.top.y, 0), 0.2f);
        }
        Gizmos.color = Color.green;
        var bottomBounds = GetPlayerBottomBounds();
        Gizmos.DrawCube(bottomBounds.center, bottomBounds.size);
    }

    bool IsOnGround() => controller.collisions.below;

    void ControllerDebug() {
        var pressed = Enumerable.Range(0, 20)
            .Select(i => new Tuple<int, bool>(i, Input.GetKeyDown("joystick button " + i)))
            .Where(tuple => tuple.Item2)
            .FirstOrDefault();

        if (pressed != null) {
            Debug.Log("joystick button " + pressed.Item1);
        }

        var axis = Input.GetAxis("X Axis");
        if (axis != 0) {
            Debug.Log("X Axis: " + axis);
        }

        var axis6 = Input.GetAxis("8th Axis");
        if (axis6 != 0) {
            Debug.Log("8th Axis: " + axis6);
        }

        var axis7 = Input.GetAxis("7th Axis");
        if (axis7 != 0) {
            Debug.Log("7th Axis: " + axis7);
        }
    }

    void VelocityDebug() {
        Debug.Log("X velocity: " + velocity.x);
        Debug.Log("Y velocity: " + velocity.y);
    }
}
