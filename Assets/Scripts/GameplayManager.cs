using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField]
    private GameplayUI _gameplayUI;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        _gameplayUI.Initialize();
    }
}
