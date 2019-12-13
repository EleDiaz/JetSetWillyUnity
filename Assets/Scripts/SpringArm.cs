using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringArm : MonoBehaviour
{
    public GameObject HeadBone;
    public Camera playerCamera;
    /** Set up an object to get their boundaries as camara constraints */
    public GameObject bounder;
    private PlayerInput playerInput;

    public float zoomSpeed = 0.5f;
    public float minDistance = 1.0f;
    public float maxDistance = 10.0f;

    public float pitchLimitsDegrees = 40;

    /** Distance defined by user input (We aim to get this)*/
    public float distance = 3.0f;
    /** Distance limited by other constraints besides max and min*/
    private float _currentDistance = 3.0f;
    private Quaternion _initialRotation = Quaternion.identity; 
    private Quaternion _currentRotation = Quaternion.identity; 
    private Quaternion _yawRotation = Quaternion.identity;
    private Quaternion _pitchRotation = Quaternion.identity;
    public float rotationSpeedX = 0.01f;
    public float rotationSpeedY = 0.01f;

    /** Seconds to automatically reposition the camera, if there aren't any movement in that time 
        it could take at least 2 times the value entered */
        /// PUFFF WRONG que se mueva el caracter no la camara
    public float cameraRepositionSeconds = 4.0f;
    private Bounds _bounds;
    /** Height defined by model mesh head position */
    private float _cameraHeight;
    private float _cameraDeltaSpace = 0.4f;

    private IEnumerator repositionCameraCourutine;
    
    void Awake() {
        playerInput = GetComponent<PlayerInput>();
        repositionCameraCourutine = RepositionCamera();
    }

    void OnEnable() {
        playerInput.onZoom += OnZoom;
        playerInput.onLookAround += OnLookAround;
        StartCoroutine(repositionCameraCourutine);
    }
    
    void OnDisable() {
        playerInput.onZoom -= OnZoom;
        playerInput.onLookAround -= OnLookAround;
        StopCoroutine(repositionCameraCourutine);
    }

    void OnZoom(float delta) => distance = Mathf.Clamp(zoomSpeed * delta + distance, minDistance, maxDistance);

    void OnLookAround(Vector2 delta2)
    {
        Quaternion yawOffset = _yawRotation * Quaternion.AngleAxis(rotationSpeedX * delta2.x, Vector3.up);
        Quaternion pitchOffset = _pitchRotation * Quaternion.AngleAxis(-rotationSpeedY * delta2.y, Vector3.right);
        
        if (Quaternion.Angle(pitchOffset, Quaternion.LookRotation(Vector3.down)) < pitchLimitsDegrees || Quaternion.Angle(pitchOffset, Quaternion.LookRotation(-Vector3.down)) < pitchLimitsDegrees) {
            _yawRotation = yawOffset;
        }
        else {
            _pitchRotation = pitchOffset;
            _yawRotation = yawOffset;
        }
        _currentRotation = _yawRotation * _initialRotation * _pitchRotation;
    }

    void Start()
    {
        if (HeadBone != null) {
            _cameraHeight = HeadBone.transform.position.y;
        }

        _initialRotation = transform.rotation;
        // Position the camera behind the player by X distance
        playerCamera.transform.position = 
            transform.position + 
            -Vector3.down * _cameraHeight + 
            _currentRotation * (-Vector3.forward * _currentDistance);

        if (bounder != null) {
            CalculateBounds();
            ConstraintToBounds();
        }
    }
    
    private IEnumerator RepositionCamera() {
        // TODO call 
        yield return new WaitForSeconds(cameraRepositionSeconds);
        RepositionCamera();
    }

    void LateUpdate()
    {
        playerCamera.transform.rotation = _currentRotation;   
        playerCamera.transform.position =
            transform.position + 
            -Vector3.down * _cameraHeight +
            _currentRotation * (-Vector3.forward * _currentDistance);

        if (bounder != null) {
            ConstraintToBounds();
        }
        // TODO: Add Raycast to avoid collide with objects in the scene
    }

    void CalculateBounds() {
        _bounds = new Bounds();
        foreach (Renderer renderer in bounder.GetComponentsInChildren<Renderer>())
        {
            _bounds.min = Vector3.Min(_bounds.min, renderer.bounds.min);
            _bounds.max = Vector3.Max(_bounds.max, renderer.bounds.max);
        }
    }

    void ConstraintToBounds() {
        Vector3 deltaSpace = new Vector3(_cameraDeltaSpace, _cameraDeltaSpace, _cameraDeltaSpace);
        Vector3 newPosition =
            Vector3.Min(_bounds.max - deltaSpace,
            Vector3.Max(_bounds.min + deltaSpace, playerCamera.transform.position));

        playerCamera.transform.position = newPosition;
        // Mathf.Atan(someangle)  *  adj
        // Vector3 adj = newPosition - playerCamera.transform.position
    }
}
