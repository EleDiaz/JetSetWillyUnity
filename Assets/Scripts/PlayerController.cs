using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player movements, and trigger the animations
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpringArm))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Max walk speed
    /// </summary>
    public float maxWalkSpeed = 1.0f;

    /// <summary>
    /// Max run speed
    /// </summary>
    public float maxRunSpeed = 2.0f;

    /// <summary>
    /// Acceleration to reach a speed, it is used on the blend animations
    /// </summary>
    public float acceleration = 0.5f;

    /// <summary>
    /// 
    /// </summary>
    public float rotationSpeed = 1.0f;

    /// <summary>
    /// Air control on jumps or falling, higher values let give you more control.
    /// </summary>
    [Range(0.0f, 1.0f)] public float airControl = 0.3f;

    /// <summary>
    /// Time to update the camera after some activity on the camera angle
    /// </summary>
    public float timeToResetCamera = 4.0f;

    public Camera playerCamera;

    private bool _isJumping = false;
    private bool _isFalling = false;
    private bool _isSprinting = false;
    private Vector2 _lerpMovementDirection = Vector2.zero;
    private Vector2 _movementDirection = Vector2.zero;
    private Vector2 _movementDirectionBeforeJump = Vector2.zero;
    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private Animator _animator;
    private SpringArm _springArm;
    private float _lastCameraCheckTime = 0.0f;

    private double TOLERANCE = 0.1;
    public int jumpForce = 300;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _springArm = GetComponent<SpringArm>();
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _playerInput.onMove += OnMove;
        _playerInput.onJump += OnJump;
        _playerInput.onMenu += OnMenu;
        _playerInput.onSprint += OnSprint;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        _playerInput.onMove -= OnMove;
        _playerInput.onJump -= OnJump;
        _playerInput.onMenu -= OnMenu;
        _playerInput.onSprint -= OnSprint;
    }

    void Start()
    {
    }

    void Update()
    {
        _lerpMovementDirection = Vector2.Lerp(_lerpMovementDirection, _isSprinting? _movementDirection * 2: _movementDirection , acceleration * Time.deltaTime);
        if (!_isJumping && !_isFalling)
        {
            Move(_lerpMovementDirection);
        }
        else if (_isJumping || _isFalling)
        {
            Move((_movementDirection * airControl + _movementDirectionBeforeJump).normalized);
        }
        UpdateAnimationMove();

        if (CheckGrounded())
        {
            if (_isFalling)
            {
                _isFalling = false;
                _animator.SetBool("IsFalling", _isFalling);
                _isJumping = false;
                _animator.SetBool("IsJumping", _isJumping);
            }
        }
        else
        {
            _isFalling = true;
            _animator.SetBool("IsFalling", _isFalling);
        }
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }

    /// <summary>
    /// Move the charater in a direction. Use the <paramref name="movementDirection"/> to get the speed and direction.
    /// </summary>
    /// <param name="movementDirection"></param>
    void Move(Vector2 movementDirection)
    {
        var moveDirection = transform.rotation * new Vector3(movementDirection.x, 0.0f, movementDirection.y);
        if (_isSprinting)
        {
            _rigidbody.MovePosition(transform.position + Mathf.Lerp(0, maxRunSpeed, movementDirection.magnitude) *
                                      Time.deltaTime * moveDirection);
        }
        else
        {
            _rigidbody.MovePosition(transform.position + Mathf.Lerp(0, maxWalkSpeed, movementDirection.magnitude) * Time.deltaTime *
                                      moveDirection);
        }

        if (Math.Abs(moveDirection.magnitude) > TOLERANCE || Time.time - _lastCameraCheckTime > timeToResetCamera)
        {
            // We could also create our yaw rotation from camera quaternion.
            //_rigidbody.MoveRotation(Quaternion.Lerp(transform.rotation.normalized, _springArm.yawRotation, rotationSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Lerp(transform.rotation.normalized, _springArm.yawRotation, rotationSpeed * Time.deltaTime);

            if (Math.Abs(Quaternion.Angle(transform.rotation, _springArm.yawRotation)) < TOLERANCE)
            {
                _lastCameraCheckTime = Time.time;
            }
        }
    }

    void UpdateHeadingPosition(Quaternion newRotation)
    {
        // TODO: Update character rotation
    }

    void UpdateHead()
    {
    }

    void UpdateAnimationMove()
    {
        if (Math.Abs(_lerpMovementDirection.magnitude) < TOLERANCE)
        {
            _animator.SetBool("IsIdle", true);
        }
        else
        {
            _animator.SetFloat("Running", _lerpMovementDirection.y);
            _animator.SetFloat("Direction", _lerpMovementDirection.x);
            _animator.SetBool("IsIdle", false);
        }
    }

    /// <summary>
    /// Set the walking speed of character
    /// </summary>
    /// <param name="vector2"> A 2 Axis input in rage of -1 to 1. It needs a normalized input vector</param>
    void OnMove(Vector2 vector2)
    {
        _movementDirection = vector2;
        UpdateAnimationMove();
    }

    void OnSprint(bool sprinting)
    {
        _isSprinting = sprinting;
        UpdateAnimationMove();
    }

    void OnJump()
    {
        if (!_isJumping)
        {
            _movementDirectionBeforeJump = _movementDirection;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _isJumping = true;
            _animator.SetBool("IsJumping", _isJumping);
        }
    }

    void OnMenu()
    {
        // Pause the game
        // Time.timeScale = 0;
        // TODO: Open the pause menu stop all components in the scene
        Cursor.lockState = CursorLockMode.None;
    }
}