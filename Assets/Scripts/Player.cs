﻿using UnityEngine;
using System.Collections;

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
    }

    void Update() {
        EarlyUpdate();

        CalculateVelocity();

        if (falling) {
            velocity.x = 0;
        }

        if (whip.Whipping && controller.collisions.below) {
            velocity.x = 0;
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (!jumped && wasOnGround && !controller.collisions.below) {
            falling = true;
        }
    }

    void LateUpdate() {
        wasOnGround = controller.collisions.below;
    }

    public void SetDirectionalInput(Vector2 input) {
        if (input.x < 0) {
            whip.WhipDirection = Whip.Direction.Left;
        } else if (input.x > 0) {
            whip.WhipDirection = Whip.Direction.Right;
        }

        if (controller.collisions.below) {
            directionalInput = input;
        }
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
}
