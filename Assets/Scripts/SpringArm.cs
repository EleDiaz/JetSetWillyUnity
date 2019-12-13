using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpringArm : MonoBehaviour
{
    /// <summary>
    /// Object to be used to attach the camera at some <paramref name="distance"/> at <paramref name="headBone"/> height
    /// </summary>
    public GameObject headBone;

    /// <summary>
    /// Camera to to attach to the player with the spring arm behavior
    /// </summary>
    public Camera playerCamera;

    /// <summary>
    /// Set up an object to get their boundaries as camara constraints
    /// </summary>
    public GameObject bounder;

    /// <summary>
    /// Zoom speed to camera
    /// </summary>
    public float zoomSpeed = 0.5f;

    public float minDistance = 1.0f;

    public float maxDistance = 10.0f;

    /// <summary>
    /// Constraint the camera view to avoid reverse the camera by going down wards or up wards
    /// </summary>
    public float pitchLimitsDegrees = 40;

    /// <summary>
    /// Distance defined by user input (We aim to get this)
    /// </summary>
    public float distance = 3.0f;
    /// <summary>
    /// Space around the camera to avoid clipping object with camera lens
    /// </summary>
    public float cameraDeltaSpace = 0.4f;

    /// <summary>
    /// Yaw rotation an easy way to extract it from the camera
    /// </summary>
    public Quaternion yawRotation = Quaternion.identity;
    /// <summary>
    /// Pitch rotation an easy way to extract it from the camera
    /// </summary>
    public Quaternion pitchRotation = Quaternion.identity;
    /// <summary>
    /// Yaw Rotation sensibility
    /// </summary>
    public float rotationSpeedX = 0.01f;
    /// <summary>
    /// Pitch Rotation sensibility
    /// </summary>
    public float rotationSpeedY = 0.01f;

    private Quaternion _initialRotation = Quaternion.identity;
    private Quaternion _currentRotation = Quaternion.identity;
    private PlayerInput _playerInput;
    private Bounds _bounds;
    /** Height defined by model mesh head position */
    private float _cameraHeight;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        _playerInput.onZoom += OnZoom;
        _playerInput.onLookAround += OnLookAround;
    }

    void OnDisable()
    {
        _playerInput.onZoom -= OnZoom;
        _playerInput.onLookAround -= OnLookAround;
    }

    void OnZoom(float delta)
    {
        distance = Mathf.Clamp(zoomSpeed * delta + distance, minDistance, maxDistance);
    }

    void OnLookAround(Vector2 delta2)
    {
        Quaternion yawOffset = yawRotation * Quaternion.AngleAxis(rotationSpeedX * delta2.x, Vector3.up);
        Quaternion pitchOffset = pitchRotation * Quaternion.AngleAxis(-rotationSpeedY * delta2.y, Vector3.right);

        if (Quaternion.Angle(pitchOffset, Quaternion.LookRotation(Vector3.down)) < pitchLimitsDegrees ||
            Quaternion.Angle(pitchOffset, Quaternion.LookRotation(-Vector3.down)) < pitchLimitsDegrees)
        {
            yawRotation = yawOffset;
        }
        else
        {
            pitchRotation = pitchOffset;
            yawRotation = yawOffset;
        }

        _currentRotation = yawRotation * _initialRotation * pitchRotation;
    }

    void Start()
    {
        if (headBone != null)
        {
            _cameraHeight = headBone.transform.position.y;
        }

        _initialRotation = transform.rotation;
        // Position the camera behind the player by X distance
        playerCamera.transform.position =
            transform.position +
            -Vector3.down * _cameraHeight +
            _currentRotation * (-Vector3.forward * distance);

        if (bounder != null)
        {
            CalculateBounds();
            ConstraintToBounds();
        }
    }

    void LateUpdate()
    {
        playerCamera.transform.rotation = _currentRotation;
        playerCamera.transform.position =
            transform.position +
            -Vector3.down * _cameraHeight +
            _currentRotation * (-Vector3.forward * distance);

        if (bounder != null)
        {
            ConstraintToBounds();
        }

        // TODO: Add Raycast to avoid collide with objects in the scene
    }

    void CalculateBounds()
    {
        _bounds = new Bounds();
        foreach (Renderer renderer in bounder.GetComponentsInChildren<Renderer>())
        {
            _bounds.min = Vector3.Min(_bounds.min, renderer.bounds.min);
            _bounds.max = Vector3.Max(_bounds.max, renderer.bounds.max);
        }
    }

    void ConstraintToBounds()
    {
        Vector3 deltaSpace = new Vector3(cameraDeltaSpace, cameraDeltaSpace, cameraDeltaSpace);
        Vector3 newPosition =
            Vector3.Min(_bounds.max - deltaSpace,
                Vector3.Max(_bounds.min + deltaSpace, playerCamera.transform.position));

        playerCamera.transform.position = newPosition;
        // We Should calculate the right position
        // Mathf.Atan(someangle)  *  adj
        // Vector3 adj = newPosition - playerCamera.transform.position
    }
}