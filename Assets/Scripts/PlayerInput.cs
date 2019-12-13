using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Helper class to solve temporally solve the problem with the UnityEvent type.
public class PlayerInput : MonoBehaviour
{
    public delegate void OnMove(Vector2 vector2);
    public event OnMove onMove;

    public delegate void OnJump();
    public event OnJump onJump;

    public delegate void OnMenu();
    public event OnMenu onMenu;

    public delegate void OnLookAround(Vector2 vector2);
    public event OnLookAround onLookAround;

    public delegate void OnZoom(float delta);
    public event OnZoom onZoom;

    // Doesnt work in the last versions of unity 
    //    public UnityEvent moving;

    private Inputs _input;

    void Awake()
    {
        _input = new Inputs();
    }

    void Move(InputAction.CallbackContext ctx) => onMove(ctx.ReadValue<Vector2>());
    void Jump(InputAction.CallbackContext _ctx) => onJump();
    void Menu(InputAction.CallbackContext _ctx) => onMenu();
    void LookAround(InputAction.CallbackContext ctx) => onLookAround(ctx.ReadValue<Vector2>());
    void Zoom(InputAction.CallbackContext ctx) => onZoom(ctx.ReadValue<float>());

    void OnEnable() {
        _input.InGame.Enable();
        _input.InGame.Move.performed += Move;
        _input.InGame.Move.canceled += Move;
        _input.InGame.Jump.performed += Jump;
        _input.InGame.Menu.performed += Menu;
        _input.InGame.LookAround.performed += LookAround;
        _input.InGame.Zoom.performed += Zoom;
    }

    void OnDisable() {
        _input.InGame.Move.performed -= Move;
        _input.InGame.Move.canceled -= Move;
        _input.InGame.Jump.performed -= Jump;
        _input.InGame.Menu.performed -= Menu;
        _input.InGame.LookAround.performed -= LookAround;
        _input.InGame.Zoom.performed -= Zoom;
        _input.InGame.Disable();
    }
}
