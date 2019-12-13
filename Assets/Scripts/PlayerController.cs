using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f; 
    /** How much faster are we going between walkSpeed and runSpeed */
    private float runningSpeedPercent = 0.0f;
    public float rotationSpeed = 1.0f;

    public bool isJumping = false;
    public bool isFalling = false;

    public float updateRotationFromCamera = 4.0f;

    private Vector2 movementDirection = Vector2.zero;

    private PlayerInput playerInput;
    private Animator animator;
    public Camera playerCamera;

    void Awake() {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
    }

    void OnEnable() {
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.onMove += OnMove;
        playerInput.onJump += OnJump;
        playerInput.onMenu += OnMenu;
    }

    void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
        playerInput.onMove -= OnMove;
        playerInput.onJump -= OnJump;
        playerInput.onMenu -= OnMenu;
    }

    void Start()
    {
    }

    void Update()
    {
        if (!isJumping && !isFalling) {
            transform.position += transform.rotation *
                new Vector3(movementDirection.x, 0.0f, movementDirection.y) * Mathf.Lerp(walkSpeed, runSpeed, runningSpeedPercent) * Time.deltaTime;

            // Quaternion newRot = playerCamera.transform.rotation;
            // newRot.x = 0;
            // newRot.z = 0;
            // transform.rotation = newRot.normalized;
        }
        else if (true /* TODO: Has Grounded */){
            // TODO: calculate velocity to go forwards
            if (isFalling) {
                isFalling = false;
                animator.SetBool("IsFalling", isFalling);
            }
            else {
                isJumping = false;
                animator.SetBool("IsJumping", isJumping);
            }
        }
    }

    void UpdateHeadingPosition(Quaternion newRotation) {
        // TODO: Update character rotation
    }

    void UpdateHead() {

    }

    void OnMove(Vector2 vector2)
    {
        // TODO: Shift + W get max speed 1.0f
        //       W get walk speed 0.5f
        // We shift the value to get 0-1 running speed to be used in lerp later
        // Quaternion newRot = playerCamera.transform.rotation;
        // newRot.x = 0;
        // newRot.z = 0;
        // transform.rotation = newRot.normalized;
        if (vector2.magnitude == 0) {
            animator.SetBool("IsIdle", true);
        }
        else {
            animator.SetFloat("Running", vector2.y);
            animator.SetFloat("Direction", vector2.x);
            animator.SetBool("IsIdle", false);
        }

        // runningSpeedPercent = 2 * vector2.magnitude - 1.0f; 
        // if (vector2.x != 0.0f && vector2.y == 0.0f && runningSpeedPercent <= 1.0f) {
        //     animator.SetFloat("Turning", vector2.x);
        // }
        movementDirection = vector2; 
    }

    void OnJump()
    {
        isJumping = true;
        animator.SetBool("IsJumping", isJumping);
    }

    void OnMenu() {
        // TODO: Open the pause menu stop all components in the scene
        Cursor.lockState = CursorLockMode.None;
    }
}
