using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;

public class GameplayUI : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _hostIp;

    [SerializeField]
    private Button _disconnectBtn;

    public void Initialize()
    {
        _disconnectBtn.onClick.AddListener(Disconnect);

        var unityTransport = FindObjectOfType<UnityTransport>();

        if (NetworkManager.Singleton.IsConnectedClient) _hostIp.gameObject.SetActive(false);
        else _hostIp.text = unityTransport.ConnectionData.Address;
    }

    public void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
