using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField]
    private SoundComponent _soundComponentTemplate;
    [SerializeField]
    private MusicComponent _musicComponentTemplate;

    [SerializeField]
    private SfxData _sfxData;
    [SerializeField]
    private MusicData _musicData;

    public event Action<bool> OnSfxEnabled;
    public event Action<bool> OnMusicEnabled;

    public event Action ClearMusicEvent;
    public event Action ClearSfxEvent;

    public bool MusicEnabled { get; private set; }
    public bool SfxEnabled { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        SfxEnabled = true;
        MusicEnabled = true;

        DontDestroyOnLoad(this);
    }

    public void PlaySFX(SFX sfx, Transform desiredTransform)
    {
        if (!SfxEnabled) return;

        var newSfx = Instantiate(_soundComponentTemplate, desiredTransform.position, Quaternion.identity, transform);
        var sfxPair = _sfxData.GetClip(sfx);
        newSfx.Initialize(sfxPair.clip, sfxPair.volume, sfxPair.loop, sfxPair.tridimensional, sfxPair.crossScene);
    }

    public void PlayRandomSFX(SFX[] sfx, Transform desiredTransform)
    {
        if (!SfxEnabled) return;

        var newSfx = Instantiate(_soundComponentTemplate, desiredTransform.position, Quaternion.identity, transform);
        var sfxPair = _sfxData.GetClip(sfx[UnityEngine.Random.Range(0, sfx.Length)]);
        newSfx.Initialize(sfxPair.clip, sfxPair.volume, sfxPair.loop, sfxPair.tridimensional, sfxPair.crossScene);
    }

    public void PlayMusic(Music music)
    {
        if (!MusicEnabled) return;

        var newMusic = Instantiate(_musicComponentTemplate, Vector3.zero, Quaternion.identity, transform);
        var musicPair = _musicData.GetClip(music);
        newMusic.Initialize(musicPair.clip, musicPair.volume);
    }

    public void EnableSFX(bool enable)
    {
        OnSfxEnabled?.Invoke(enable);
        SfxEnabled = enable;
    }

    public void EnableMusic(bool enable)
    {
        OnMusicEnabled?.Invoke(enable);
        MusicEnabled = enable;
    }

    public void ClearSfx()
    {
        ClearSfxEvent?.Invoke();
    }

    public void ClearMusic()
    {
        ClearMusicEvent?.Invoke();
    }
}

public enum SFX
{
    STEPS_1,
    STEPS_2,
    SHOT,
    LOADING_SHOT,
    WATER_LOSE,
    BALL_HIT,
    WAVES,
    BUTTON_CLICK,
}

public enum Music
{
    LOBBY_MUSIC_1,
    LOBBY_MUSIC_2,
    GAMEPLAY_MUSIC_1,
    GAMEPLAY_MUSIC_2,
    WIN_MUSIC_1,
    WIN_MUSIC_2,
}
