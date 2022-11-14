using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "SfxData", menuName = "SfxData")]
public class SfxData : ScriptableObject
{
    [SerializeField]
    private List<SfxPair> _sfxPairs;

    public SfxPair GetClip(SFX name)
    {
        var clip = _sfxPairs.Find(sfx => sfx.name == name);
        return clip;
    }
}

[System.Serializable]
public struct SfxPair
{
    public SFX name;
    public AudioClip clip;
    public float volume;
    public bool loop;
    public bool tridimensional;
    public bool crossScene;
}
