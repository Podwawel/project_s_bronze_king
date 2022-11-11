using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _localPlayerNickname;

    [SerializeField]
    private TextMeshProUGUI _fps;
    private float _deltaTime = 0f;
    private float _updateTime = 0f;

    [SerializeField]
    private TextMeshProUGUI _ping;

    public void Initialize()
    {
        _localPlayerNickname.text = SessionData.instance.LocalPlayerNickname;
    }

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _updateTime += Time.deltaTime;

        if (_updateTime < 0.15f) return;

        var msec = _deltaTime * 1000.0f;
        var fps = 1.0f / _deltaTime;
        var text = string.Format("{0:0} fps ({1:0.0} ms)", fps, msec);
        _fps.text = text;
        _updateTime = 0f;
        var ping = NetworkManager.Singleton.LocalTime - NetworkManager.Singleton.ServerTime;
        _ping.text = string.Format("Ping: {0:0.0} ms", ping.TimeAsFloat * 1000.0f);
    }
}
