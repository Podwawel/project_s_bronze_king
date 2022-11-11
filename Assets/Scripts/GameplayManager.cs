using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField]
    private GameplayNetworkingUI _gameplayNetworkingUI;

    [SerializeField]
    private GameplayUI _gameplayUI;

    [SerializeField]
    private WinCamera _winCamera;
    [SerializeField]
    private DeathCamera _deathCamera;

    public DeathCamera DeathCamera => _deathCamera;
    public WinCamera WinCamera => _winCamera;

    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 60;
        _gameplayNetworkingUI.Initialize();
    }

    public void InitializeUI()
    {
        _gameplayUI.Initialize();
    }

    public void TriggerWin(string nickname)
    {
        _gameplayNetworkingUI.ShowWinnerMessage(nickname);
    }
}
