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
    [SerializeField]
    private List<Transform> _wavesSoundPlace;

    public DeathCamera DeathCamera => _deathCamera;
    public WinCamera WinCamera => _winCamera;

    protected override void Awake()
    {
        base.Awake();

        SoundManager.instance.ClearSfx();
        SoundManager.instance.ClearMusic();
        SoundManager.instance.PlayMusic(Music.GAMEPLAY_MUSIC_2);
        foreach(var wave in _wavesSoundPlace)
        {
            SoundManager.instance.PlaySFX(SFX.WAVES, wave);
        }
        Application.targetFrameRate = 60;
        _gameplayNetworkingUI.Initialize();
    }

    public void InitializeUI()
    {
        _gameplayUI.Initialize();
    }

    public void TriggerWin(string nickname, string time)
    {
        _gameplayNetworkingUI.ShowWinnerMessage(nickname, time);
    }
}
