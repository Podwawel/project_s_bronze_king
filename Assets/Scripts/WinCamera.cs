using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    public void OnWin()
    {
        _camera.enabled = true;
    }
}
