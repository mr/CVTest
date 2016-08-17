using UnityEngine;

public class GameManager : MonoBehaviour {

    private float playerGravity;
    public float PlayerGravity {
        set {
            playerGravity = value;
        }

        get {
            return playerGravity;
        }
    }

    void Start () {
    }

    void Update () {

    }

    public static GameManager Instance {
        get {
            return GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
    }
}
