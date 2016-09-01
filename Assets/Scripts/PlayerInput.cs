using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {

    Player player;

    void Start() {
        player = GetComponent<Player>();
    }

    void Update() {
        var directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.DirectionalInput = directionalInput;

        if (Input.GetKeyDown(KeyCode.A)) {
            player.StairDirectionalInput = Vector2.left;
        } else if (Input.GetKeyDown(KeyCode.S)) {
            player.StairDirectionalInput = Vector2.right;
        } else {
            player.StairDirectionalInput = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            player.OnJumpInputUp();
        }
    }
}
