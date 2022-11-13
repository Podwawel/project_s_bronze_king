using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SessionData : Singleton<SessionData>
{
    [HideInInspector] public string hostIp;
    private Transform _localPlayerTransform;

    private string _localPlayerNickname;
    private Vector3 _localPlayerColor;


    public bool ServerAuthorizedBallShooting { get; set; }
    public string LocalPlayerNickname => _localPlayerNickname;
    public Vector3 LocalPlayerColor => _localPlayerColor;

    public void Initialize()
    {
        ServerAuthorizedBallShooting = true;
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

    public void CreateLocalPlayerColor()
    {
        _localPlayerColor = new Vector3(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
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
