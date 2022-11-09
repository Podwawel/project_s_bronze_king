using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SessionData : Singleton<SessionData>
{
    [HideInInspector] public string hostIp;
    private Transform _localPlayerTransform;

    private string _localPlayerNickname;

    public string LocalPlayerNickname => _localPlayerNickname;

    public void Initialize()
    {
        DontDestroyOnLoad(this);
    } 

    public void AssignLocalPlayerTransform(Transform transform)
    {
        _localPlayerTransform = transform;
    }

    public void AssignLocalPlayerNickname(string nickname)
    {
        _localPlayerNickname = nickname;
    }

    public void ClearData()
    {
        hostIp = string.Empty;
    }

    public void RotateTowardsLocalPlayer(Transform transform)
    {
        transform.LookAt(_localPlayerTransform);
    }
}
