using UnityEngine;

public class CameraBarrier : MonoBehaviour {

    public bool switcher = false;

    void Start() { }

    void Update() { }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));
    }
}
