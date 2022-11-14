using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "MusicData")]
public class MusicData : ScriptableObject
{
    [SerializeField]
    private List<MusicPair> _musicPairs;

    public MusicPair GetClip(Music name)
    {
        var clip = _musicPairs.Find(music => music.name == name);
        return clip;
    }
}

[System.Serializable]
public struct MusicPair
{
    public Music name;
    public AudioClip clip;
    public float volume;
}