using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using Unity.BossRoom.Infrastructure;

public class TenisBallCannon : NetworkBehaviour
{
    public event Action OnShotLoadedEvent;
    public event Action OnShotPerformedEvent;

    [Space, SerializeField]
    private Transform _shotPoint;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private float _shotPower;

    [SerializeField]
    private GameObject _ammoPrefab;
    [SerializeField]
    private GameObject _ammoLocalPrefab;
    [Space, SerializeField]
    private float _cooldown;
    [SerializeField]
    private float _loadTime;
    public float LoadTime => _loadTime;

    private bool _setCooldown = false;
    private NetworkObjectPool _pool;

    private NetworkObject _ammoToDespawn;

    public void Initialize()
    {
        _pool = FindObjectOfType<NetworkObjectPool>();
        _pool.InitializePool();
    }

    public void Shot()
    {
        if (_setCooldown) return;

        StartCoroutine(ShotAfterLoad());
        OnShotLoadedEvent?.Invoke();
    }

    private IEnumerator ShotAfterLoad()
    {
        yield return new WaitForSeconds(_loadTime);

        _setCooldown = true;

        SpawnTenisBallServerRpc(NetworkManager.LocalClientId);
        //SpawnTenisBallLocal();
        StartCoroutine(CountCooldown());

        OnShotPerformedEvent?.Invoke();
    }

    #region SERVER_SIDE
    [ServerRpc]
    public void SpawnTenisBallServerRpc(ulong id)
    {
        _pool = FindObjectOfType<NetworkObjectPool>();
        var ammo = _pool.GetNetworkObject(_ammoPrefab);

        ammo.transform.position = _shotPoint.transform.position;

        ammo.Spawn();
        ShotBall(ammo, id);

        StartCoroutine(ReturnTenisBallToPool(ammo));
    }

    public void ShotBall(NetworkObject ammoNet, ulong id)
    {
        ammoNet.transform.position = _shotPoint.transform.position;

        var ammo = ammoNet.GetComponent<TenisBall>();

        ammo.Shoot(_shotPoint.forward, _shotPoint.localPosition, _shotPower, id);
    }

    public IEnumerator ReturnTenisBallToPool(NetworkObject ammo)
    {
        yield return new WaitForSeconds(8);
        _ammoToDespawn = ammo;
        ReturnAmmo();
    }

    private void ReturnAmmo()
    {
        _pool.ReturnNetworkObject(_ammoToDespawn, _ammoPrefab);
        _ammoToDespawn.Despawn(false);
    }
    #endregion

  /*  #region CLIENT_SIDE
    public void SpawnTenisBallLocal()
    {
        var ammo = Instantiate(_ammoLocalPrefab);
        ammo.transform.position = _shotPoint.transform.position;
        var tenisBall = ammo.GetComponent<TenisBall>();

        var otherClients = new List<ulong>();
        var players = FindObjectsOfType<PlayerController>();

        foreach(var player in players)
        {
            otherClients.Add(player.myId);
            Debug.Log(player.myId);
        }    

        otherClients.Remove(NetworkManager.LocalClientId);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = otherClients
            }
        };

        SpawnTenisBallClientRpc(_shotPoint.position, _shotPoint.forward, clientRpcParams);

        tenisBall.Shoot(_shotPoint.forward, _shotPoint.localPosition, _shotPower);
    }

    [ClientRpc]
    public void SpawnTenisBallClientRpc(Vector3 spawn, Vector3 direction, ClientRpcParams clientParams = default)
    {
        var ammo = Instantiate(_ammoLocalPrefab);
        ammo.transform.position = spawn;
        var tenisBall = ammo.GetComponent<TenisBall>();

        tenisBall.Shoot(direction, spawn, _shotPower);
    }

    #endregion*/

    public IEnumerator CountCooldown()
    {
        yield return new WaitForSeconds(_cooldown);
        _setCooldown = false;
    }
}
