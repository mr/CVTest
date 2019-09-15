using UnityEngine;
using Enums;

namespace Enums {
    public enum Direction {
        Left, Right
    }
}

public static class Util {
    public static Vector2 DirectionToVector2(Direction direction) {
        switch (direction) {
            case Direction.Left:
                return Vector2.left;
            default:
                return Vector2.right;
        }
    }

    public static Direction SignToDirection(float sign) {
        if (Mathf.Sign(sign) < 0) {
            return Direction.Left;
        } else {
            return Direction.Right;
        }
    }

    public static float DirectionToSign(Direction direction) {
        if (direction == Direction.Left) {
            return -1;
        } else {
            return 1;
        }
    }

    public static Direction BoolToDirection(bool boolean) {
        if (!boolean) {
            return Direction.Left;
        } else {
            return Direction.Right;
        }
    }

    public static Direction DirectionOf(GameObject of, GameObject relativeTo) {
        if (of.transform.position.x < relativeTo.transform.position.x) {
            return Direction.Left;
        } else {
            return Direction.Right;
        }
    }
}