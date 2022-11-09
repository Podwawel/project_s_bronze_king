using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private SessionData _sessionData;
    [SerializeField]
    private NetworkManagerUI _networkUI;
    [SerializeField]
    private SceneLoader _sceneLoader;

    private void Awake()
    {
        _sessionData.Initialize();
        _sceneLoader.Initialize();
        _networkUI.Initialize();
    }
}
