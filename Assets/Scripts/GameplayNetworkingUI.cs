using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;
using DG.Tweening;
using UnityEngine.UI;

public class GameplayNetworkingUI : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _hostIp;

    [SerializeField]
    private Button _disconnectBtn;

    [SerializeField]
    private CanvasGroup _winnerMsgPanel;
    [SerializeField]
    private TextMeshProUGUI _winnerText;

    private UnityTransport _unityTransport;

    public void Initialize()
    {
        _disconnectBtn.onClick.AddListener(Disconnect);

        _unityTransport = FindObjectOfType<UnityTransport>();

        if (NetworkManager.Singleton.IsConnectedClient) _hostIp.gameObject.SetActive(false);
        else _hostIp.text = _unityTransport.ConnectionData.Address;
    }

    public void ShowWinnerMessage(string nickname)
    {
        _winnerText.text = nickname + " grabbed a crown!";
        _winnerMsgPanel.DOFade(1, 1f);
    }

    public void Disconnect()
    {
        //if(IsLocalPlayer) NetworkManager.Singleton.Shutdown();
        //NetworkManager.Singleton.Shutdown();
        if (IsHost) NetworkManager.Shutdown();
        else ShutdownServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShutdownServerRpc(ulong id)
    {
        NetworkManager.DisconnectClient(id);
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
