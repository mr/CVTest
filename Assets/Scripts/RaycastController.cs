﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;
    public LayerMask climbingStairsCollisionMask;

    public const float skinWidth = .015f;
    const float dstBetweenRays = .15f;
    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public Collider2D boxCollider;
    public RaycastOrigins raycastOrigins;

    public virtual void Awake() {
        boxCollider = GetComponent<Collider2D>();
    }

    public virtual void Start() {
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing() {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
