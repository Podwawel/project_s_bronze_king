using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicComponent : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    private void Awake()
    {
        SoundManager.instance.OnMusicEnabled += OnMusicStateChange;
        SoundManager.instance.ClearMusicEvent += OnStopPlaying;
    }

    public void Initialize(AudioClip clip, float volume)
    {
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.Play();
        //Invoke("OnStopPlaying", _audioSource.clip.length);
    }

    public void OnMusicStateChange(bool state)
    {
        if (state) _audioSource.Play();
        else _audioSource.Stop();
    }

    private void OnStopPlaying()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SoundManager.instance.OnMusicEnabled -= OnMusicStateChange;
        SoundManager.instance.ClearMusicEvent -= OnStopPlaying;
    }
}
