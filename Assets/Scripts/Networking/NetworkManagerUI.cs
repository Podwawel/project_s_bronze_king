using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button _clientBtn;
    [SerializeField]
    private Button _hostBtn;
    [SerializeField]
    private Button _manualHostBtn;

    [Space, SerializeField]
    private Button _joinLocalHostBtn;
    [Space, SerializeField]
    private Button _startLocalHostBtn;

    [Space, SerializeField]
    private GameObject _wrongIPMessage;
    [SerializeField]
    private GameObject _wrongPortMessage;
    [SerializeField]
    private GameObject _wrongNameMessage;

    [Space, SerializeField]
    private TMP_InputField _IPInputField;
    [SerializeField]
    private TMP_InputField _portInputField;
    [SerializeField]
    private TMP_InputField _nicknameInputField;

    private UnityTransport _unityTransport;

    private bool _connectionDataIPCorrect = true;
    private bool _connectionDataPortCorrect = true;
    private bool _connectionDataNameCorrect = true;

    public void Initialize()
    {
        _portInputField.text = "7777";

        _unityTransport = FindObjectOfType<UnityTransport>();

        _startLocalHostBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);
            SceneLoader.instance.ChangeScene(2);

            SetLocalHost();
            SceneLoader.instance.SubscribeOnSceneLoad(JoinAsHost);
        });
        _joinLocalHostBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);
            SceneLoader.instance.ChangeScene(2);

            SetLocalHost();
            SceneLoader.instance.SubscribeOnSceneLoad(JoinAsClient);
        });
        _hostBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);
            var ipAddress = GetLocalIPAddress();
            UInt16 port = 7777;
            var nickname = _nicknameInputField.text;

            CheckConnectionAvaiability();

            if (!_connectionDataIPCorrect || !_connectionDataPortCorrect || !_connectionDataNameCorrect) return;

            var portConverted = port;

            SetConnectionData(ipAddress, portConverted);
            SessionData.instance.hostIp = ipAddress;
            SceneLoader.instance.ChangeScene(2);

            SceneLoader.instance.SubscribeOnSceneLoad(JoinAsHost);
        });
        _manualHostBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);
            var ipAddress = _IPInputField.text;
            var port = _portInputField.text;
            var nickname = _nicknameInputField.text;

            CheckConnectionAvaiability();

            if (!_connectionDataIPCorrect || !_connectionDataPortCorrect || !_connectionDataNameCorrect) return;

            var portConverted = UInt16.Parse(port);

            SetConnectionData(ipAddress, portConverted);
            SessionData.instance.hostIp = ipAddress;

            SceneLoader.instance.ChangeScene(2);

            SceneLoader.instance.SubscribeOnSceneLoad(JoinAsHost);
        });
        _clientBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);
            var ipAddress = _IPInputField.text;
            var port = _portInputField.text;

            CheckConnectionAvaiability();

            if (!_connectionDataIPCorrect || !_connectionDataPortCorrect || !_connectionDataNameCorrect) return;

            var portConverted = UInt16.Parse(port);

            SetConnectionData(ipAddress, portConverted);

            SceneLoader.instance.ChangeScene(2);

            SceneLoader.instance.SubscribeOnSceneLoad(JoinAsClient);
        });
    }

    private void JoinAsClient(Scene scene, LoadSceneMode mode)
    {
        
        SessionData.instance.AssignLocalPlayerNickname(_nicknameInputField.text);
        SessionData.instance.CreateLocalPlayerColor();
        NetworkManager.Singleton.StartClient();
        SceneLoader.instance.UnsubscribeOnSceneLoad(JoinAsClient);
    }

    private void JoinAsHost(Scene scene, LoadSceneMode mode)
    {
        SessionData.instance.AssignLocalPlayerNickname(_nicknameInputField.text);
        SessionData.instance.CreateLocalPlayerColor();
        NetworkManager.Singleton.StartHost();
        SceneLoader.instance.UnsubscribeOnSceneLoad(JoinAsHost);
    }

    private void SetLocalHost()
    {
        SetConnectionData("127.0.0.1", 7777);
    }

    private void CheckConnectionAvaiability()
    {
        _connectionDataPortCorrect = true;
        _connectionDataIPCorrect = true;
        _connectionDataNameCorrect = true;

        _wrongIPMessage.SetActive(false);
        _wrongPortMessage.SetActive(false);
        _wrongNameMessage.SetActive(false);

        CheckIpInputField(_IPInputField);
        CheckPortInputField(_portInputField);
        CheckNameInputField(_nicknameInputField);
    }

    public void CheckIpInputField(TMP_InputField field)
    {
        var content = field.text;

        if (string.IsNullOrEmpty(content) || content.Length < 7 || content.Length > 15)
        {
            _wrongIPMessage.SetActive(true);
            _connectionDataIPCorrect = false;
            return;
        }

        foreach (var sign in content)
        {
            if (!char.IsDigit(sign) && sign != '.')
            {
                _connectionDataIPCorrect = false;
                _wrongIPMessage.SetActive(true);
                return;
            }
        }
    }

    public void CheckPortInputField(TMP_InputField field)
    {
        var content = field.text;

        if (string.IsNullOrEmpty(content))
        {
            _wrongPortMessage.SetActive(true);
            _connectionDataPortCorrect = false;
            return;
        }

        foreach (var sign in content)
        {
            if (!char.IsDigit(sign))
            {
                _connectionDataPortCorrect = false;
                _wrongPortMessage.SetActive(true);
                return;
            }
        }
    }

    public void CheckNameInputField(TMP_InputField field)
    {
        var content = field.text;

        if (string.IsNullOrEmpty(content) || content.Length > 12 || content.Length < 3)
        {
            _wrongNameMessage.SetActive(true);
            _connectionDataNameCorrect = false;
            return;
        }

        foreach (var sign in content)
        {
            if (char.IsWhiteSpace(sign))
            {
                _connectionDataPortCorrect = false;
                _wrongPortMessage.SetActive(true);
                return;
            }
        }
    }

    private void SetConnectionData(string ipAddress, ushort port)
    {
        _unityTransport.ConnectionData.Address = ipAddress;
        _unityTransport.ConnectionData.Port = port;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                SessionData.instance.hostIp = ip.ToString();
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
