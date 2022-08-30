using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float speed = 20.0f;
    private Vector2 inputVector;
    private Rigidbody2D rbody;

    private void Awake() {
        rbody = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context) {
        if (context.performed) {
            inputVector = context.ReadValue<Vector2>();
        } else {
            inputVector = Vector2.zero;
        }
    }

    public void FixedUpdate() {
        rbody.MovePosition(rbody.position + (inputVector * speed) * Time.fixedDeltaTime);
    }
}
