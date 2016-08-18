using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform target;

    void Start () { }

    void Update () {
        var oldPos = transform.position;
        oldPos.x = target.position.x;
        transform.position = oldPos;
    }
}
