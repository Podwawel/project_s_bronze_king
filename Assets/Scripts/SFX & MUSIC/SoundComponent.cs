using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundComponent : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    private void Awake()
    {
        SoundManager.instance.OnSfxEnabled += OnSfxStateChange;
    }

    public void Initialize(AudioClip clip, float volume, bool loop, bool tridim, bool crossScene)
    {
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.Play();
        _audioSource.loop = loop;

        if (tridim)
        {
            _audioSource.spatialBlend = 1;
        }

        if(!crossScene) SoundManager.instance.ClearSfxEvent += OnStopPlaying;

        if(!loop) Invoke("OnStopPlaying", _audioSource.clip.length);
    }

    public void OnSfxStateChange(bool state)
    {
        if(state) _audioSource.Play();
        else _audioSource.Stop();
    }

    private void OnStopPlaying()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SoundManager.instance.OnSfxEnabled -= OnSfxStateChange;
        SoundManager.instance.ClearSfxEvent -= OnStopPlaying;
    }
}
