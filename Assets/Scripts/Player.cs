using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Enums;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
    public const string Tag = "Player";

    public LayerMask stairMask;

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    public float moveSpeed = 6;
    public float playerFootSize = 0.3f;
    public float recoilHeight = 2;
    public float timeToRecoilApex = .4f;
    public float recoilSpeed = 5;
    public float stairSpeed = 6;

    public int health;
    public int maxHealth = 10;

    private bool wasOnGround;
    private bool jumped;
    private bool falling;
    private bool climbingStairs;
    private bool recoiling;

    private int numOnStairs;
    private bool touchingStairs => numOnStairs > 0;

    private void addStairs() {
        numOnStairs++;
    }

    private void removeStairs() {
        numOnStairs--;
    }

    private float gravity;
    private float jumpVelocity;
    private float recoilGravity;
    private float recoilVelocity;
    private Vector3 velocity;

    private Controller2D controller;

    private new Collider2D collider;

    private SpriteRenderer spriteRenderer;

    private Vector2 directionalInput;
    private Vector2 DirectionalInput {
        set {
            if (value.x < 0) {
                whip.WhipDirection = Direction.Left;
                spriteRenderer.flipX = false;
            } else if (value.x > 0) {
                whip.WhipDirection = Direction.Right;
                spriteRenderer.flipX = true;
            }

            directionalInput = value;
        }
        get => directionalInput;
    }

    private Stairs validStair;
    private readonly HashSet<Stairs> stairEnds = new HashSet<Stairs>();

    private LerpState? stairLerpState;

    private readonly struct LerpState {
        private readonly float startTime;
        private readonly float length;
        public readonly Vector2 start;
        public readonly Vector2 end;

        public LerpState(Vector2 start, Vector2 end) {
            this.start = start;
            this.end = end;

            startTime = Time.time;
            length = Vector2.Distance(start, end);
        }

        public float FracJourney(float time, float speed) {
            var distCovered = (time - startTime) * speed;
            return distCovered / length;
        }
    }

    private Direction? enemyHitDirection;
    public int invulnerabilityDuration = 5;
    private float invulnerabilityDurationState;

    public Whip whip;

    private Slider healthSlider;

    public SceneLoader.SceneLoader sceneLoader;

    void Start() {
        controller = GetComponent<Controller2D>();
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        recoilGravity = -(2 * recoilHeight) / Mathf.Pow(timeToRecoilApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        recoilVelocity = Mathf.Abs(recoilGravity) * timeToRecoilApex;
        GameManager.Instance.PlayerGravity = gravity;
        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        health = maxHealth;
    }

    void Update() {
        if (!sceneLoader.ready) {
            return;
        }

        GetInput();

        if (IsOnGround() && velocity.y <= 0) {
            jumped = false;
            falling = false;
            recoiling = false;
        }

        CalculateVelocity();

        if (enemyHitDirection != null) {
            velocity.x = Util.DirectionToSign(enemyHitDirection.Value) * recoilSpeed;
            velocity.y = recoilVelocity;
            enemyHitDirection = null;
            recoiling = true;
        }

        if (!climbingStairs) {
            StartClimbingIfAble();
        }

        if (climbingStairs) {
            ClimbStairs();
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

        // falling refers to walking off an edge, not falling from a jump
        if (!jumped && !recoiling && wasOnGround && !IsOnGround() && !climbingStairs) {
            falling = true;
        }

        if (climbingStairs && !touchingStairs) {
            climbingStairs = false;
        }
    }

    void LateUpdate() {
        wasOnGround = IsOnGround();
        if (invulnerabilityDurationState == 0) {
            return;
        }
        invulnerabilityDurationState -= Time.deltaTime;
        if (invulnerabilityDurationState <= 0) {
            invulnerabilityDurationState = 0;
        }
    }

    private void CalculateVelocity() {
        if (IsOnGround() || climbingStairs) {
            velocity.x = DirectionalInput.x * moveSpeed;
        }
        velocity.y += gravity * Time.deltaTime;
    }

    private void StartClimbingIfAble() {
        validStair = stairEnds
            .FirstOrDefault(stair =>
                stair.allowedToStartClimb(GetPlayerBottom(), GetPlayerBottomBounds(), DirectionalInput)
                    && stair.collider.bounds.Intersects(GetPlayerBottomBounds())
                    && (IsOnGround() || climbingStairs));
        climbingStairs = touchingStairs && validStair != null;
        // climbing stairs for the first time
        if (!climbingStairs) {
            return;
        }
        var topOrBottom = validStair.IsHeadingUp(DirectionalInput) ? validStair.bottom : validStair.top;
        stairLerpState = new LerpState(GetPlayerBottom(), topOrBottom);
    }

    private void ClimbStairs() {
        // Default to not moving on the stairs (aka don't fall through the stairs)
        velocity = Vector2.zero;
        if (stairLerpState != null) {
            if (DirectionalInput != Vector2.zero) {
                var start = stairLerpState.Value.start;
                var end = stairLerpState.Value.end;
                var fracJourney = stairLerpState?.FracJourney(Time.time, stairSpeed);
                if (GetPlayerBottom() != end) {
                    var newBottomPos = Vector2.Lerp(start, end, fracJourney.Value);
                    SetPlayerBottom(newBottomPos);
                } else {
                    // finished lerping to start of stairs
                    stairLerpState = null;
                }
            }
        }

        if (stairLerpState != null) {
            return;
        }
        var isYInput = DirectionalInput.y != 0;
        velocity.x = velocity.y =  (isYInput ? DirectionalInput.y : DirectionalInput.x) * stairSpeed;
        if (validStair != null && validStair.grade == Stairs.Grade.Down) {
            if (isYInput) {
                velocity.x *= -1;
            } else {
                velocity.y *= -1;
            }
        }

        if (!WillLeaveStairs(velocity * Time.deltaTime)) {
            return;
        }
        velocity = Vector2.zero;
        var currentStairs = GetEndStairsPlayerIsOn();
        var topOrBottom = currentStairs.IsHeadingUp(DirectionalInput)
            ? currentStairs.top
            : currentStairs.bottom;
        transform.position = new Vector3(topOrBottom.x, topOrBottom.y + collider.bounds.extents.y, 0);
        StartClimbingIfAble();
    }

    private bool WillLeaveStairs(Vector3 newVelocity) {
        var newVelocity2 = Vector3s.toVector2(newVelocity);
        var collidedStairs = Physics2D.OverlapPoint(GetPlayerBottom() + newVelocity2, layerMask: stairMask);
        return collidedStairs == null;
    }

    private Vector2 GetPlayerBottom() {
        var bounds = collider.bounds;
        return new Vector2(bounds.center.x, bounds.min.y);
    }

    private void SetPlayerBottom(Vector2 newPos) {
        var oldPos = transform.position;
        transform.position = new Vector3(newPos.x, newPos.y + collider.bounds.extents.y, oldPos.z);
    }

    private Bounds GetPlayerBottomBounds() {
        return new Bounds(GetPlayerBottom(), new Vector2(collider.bounds.size.x, playerFootSize));
    }

    private Stairs GetEndStairsPlayerIsOn() =>
        stairEnds.FirstOrDefault(
            stairEnd => stairEnd.collider.bounds.Contains(GetPlayerBottom()));

    private bool IsInvulnerable() {
        return invulnerabilityDurationState > 0;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (Layers.ENEMY == other.gameObject.layer && !IsInvulnerable()) {
            var enemy = other.gameObject.GetComponent<IEnemy>();
            health -= enemy.GetDamage();
            if (health < 0) {
                health = 0;
            }
            healthSlider.value = health / (float) maxHealth;
            // away from the enemy
            enemyHitDirection = Util.DirectionOf(gameObject, other.gameObject);
            invulnerabilityDurationState = invulnerabilityDuration;
        }

        if (other.gameObject.CompareTag(Stairs.TAG)) {
            var stairs = other.gameObject.GetComponent<Stairs>();
            OnStairsTriggerEnter(other, stairs);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag(Stairs.TAG)) {
            var stairs = other.gameObject.GetComponent<Stairs>();
            OnStairsTriggerExit(other, stairs);
        }
    }

    private void OnStairsTriggerEnter(Collider2D other, Stairs stairs) {
        // Tile stairs are null
        if (stairs != null) {
            stairEnds.Add(stairs);
        }

        addStairs();
    }

    private void OnStairsTriggerExit(Collider2D other, Stairs stairs) {
        removeStairs();
        if (stairs != null) {
            stairEnds.Remove(stairs);
        }

        if (!touchingStairs) {
            stairEnds.Clear();
        }
    }

    private void GetInput() {
        var gamepad = Gamepad.current;

        // prioritize dpad :^)
        var dpadLeft = -1 * gamepad.dpad.left.ReadValue();
        var dpadRight = gamepad.dpad.right.ReadValue();
        var dpadUp = gamepad.dpad.up.ReadValue();
        var dpadDown = -1 * gamepad.dpad.down.ReadValue();
        DirectionalInput = new Vector2(dpadLeft + dpadRight, dpadUp + dpadDown);

        if (gamepad.aButton.wasPressedThisFrame) {
            OnJumpInputDown();
        }

        if (gamepad.xButton.wasPressedThisFrame) {
            whip.DoWhip();
        }
    }

    private  void OnJumpInputDown() {
        if (!IsOnGround()) {
            return;
        }
        velocity.y = jumpVelocity;
        jumped = true;
    }

    void OnDrawGizmos() {
        if (!Application.IsPlaying(gameObject)) {
            return;
        }
        Gizmos.color = Color.red;
        if (validStair != null) {
            Gizmos.DrawSphere(new Vector3(validStair.bottom.x, validStair.bottom.y, 0), 0.2f);
            Gizmos.DrawSphere(new Vector3(validStair.top.x, validStair.top.y, 0), 0.2f);
        }
        Gizmos.color = Color.green;
        var bottomBounds = GetPlayerBottomBounds();
        Gizmos.DrawCube(bottomBounds.center, bottomBounds.size);
    }

    private bool IsOnGround() => controller.collisions.below;

    private void ControllerDebug() {
        var pressed = Enumerable.Range(0, 20)
            .Select(i => new Tuple<int, bool>(i, Input.GetKeyDown("joystick button " + i)))
            .FirstOrDefault(tuple => tuple.Item2);

        if (pressed != null) {
            Debug.Log("joystick button " + pressed.Item1);
        }

        var axis = Input.GetAxis("X Axis");
        if (axis != 0) {
            Debug.Log("X Axis: " + axis);
        }

        var axis6 = Input.GetAxis("6th Axis");
        if (axis6 != 0) {
            Debug.Log("6th Axis: " + axis6);
        }

        var axis7 = Input.GetAxis("7th Axis");
        if (axis7 != 0) {
            Debug.Log("7th Axis: " + axis7);
        }
    }

    private void VelocityDebug() {
        Debug.Log("X velocity: " + velocity.x);
        Debug.Log("Y velocity: " + velocity.y);
    }
}
