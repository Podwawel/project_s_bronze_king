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

    [SerializeField]
    private Texture2D _cursorTexture;

    private void Awake()
    {
        Cursor.SetCursor(_cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
        SoundManager.instance.ClearSfx();
        SoundManager.instance.ClearMusic();
        SoundManager.instance.PlayMusic(Music.LOBBY_MUSIC_1);
        _sessionData.Initialize();
        _sceneLoader.Initialize();
        _networkUI.Initialize();
    }
}
