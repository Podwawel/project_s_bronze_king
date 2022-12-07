using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{
    [SerializeField]
    private PlayerInputController _playerInputController;
    public bool CursorLocked { get; private set; }

    public void Initialize()
    {
        _playerInputController.OnCursorLockPerformed += SwitchCursorLockState;
        SwitchCursorLockState();
    }

    public void SwitchCursorLockState()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            CursorLocked = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            CursorLocked = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ForceCursorVisible()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        _playerInputController.OnCursorLockPerformed -= SwitchCursorLockState;
    }
}
