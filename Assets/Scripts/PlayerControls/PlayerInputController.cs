using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    private PlayerInput _input;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;
    private InputAction _fireAction;
    private InputAction _CursorLockAction;

    public event Action OnJumpPerformed;
    public event Action OnFirePerformed;
    public event Action OnCursorLockPerformed;

    public Vector3 MoveVectorValue { get { return new Vector3(_moveAction.ReadValue<Vector2>().x, 0f, _moveAction.ReadValue<Vector2>().y); } }
    public Vector2 LookVectorValue { get { return _lookAction.ReadValue<Vector2>(); } }

    public void InitializeInput()
    {
        _moveAction = _input.actions["Move"];
        _lookAction = _input.actions["Look"];
        _fireAction = _input.actions["Fire"];
        _jumpAction = _input.actions["Jump"];
        _CursorLockAction = _input.actions["CursorLock"];

        _jumpAction.performed += action => OnJumpPerformed?.Invoke();
        _fireAction.performed += action => OnFirePerformed?.Invoke();
        _CursorLockAction.performed += action => OnCursorLockPerformed?.Invoke();
    }
}
