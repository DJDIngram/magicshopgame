using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDataPersistence
{

    public float speed = 20.0f;
    private Vector2 inputVector;
    private Rigidbody2D rbody;

    private void Awake() {
        rbody = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context) {
        if (context.performed && GameStateManager.instance.gameState == GameState.Playing) {
            inputVector = context.ReadValue<Vector2>();
        } else {
            inputVector = Vector2.zero;
        }
    }

    public void FixedUpdate() {
        rbody.MovePosition(rbody.position + (inputVector * speed) * Time.fixedDeltaTime);
    }

    public void LoadData(GameData data) { gameObject.transform.position = data.playerPosition; }
    public void SaveData(GameData data) { data.playerPosition = gameObject.transform.position; }
}
