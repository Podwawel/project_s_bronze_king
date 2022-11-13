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

        if(SessionData.instance.ServerAuthorizedBallShooting) SpawnTenisBallServerRpc(NetworkManager.LocalClientId); // method for server authorized ball shooting
        else SpawnTenisBallLocal(); // method for client authorized ball shooting
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

    #region CLIENT_SIDE
    public void SpawnTenisBallLocal()
    {
        var ammo = Instantiate(_ammoLocalPrefab);
        ammo.transform.position = _shotPoint.transform.position;
        var tenisBall = ammo.GetComponent<TenisBall>();


        SpawnTenisBallServerRpc(_shotPoint.position, _shotPoint.forward, NetworkManager.LocalClientId);

        tenisBall.Shoot(_shotPoint.forward, _shotPoint.localPosition, _shotPower, NetworkManager.LocalClientId);
    }

    [ClientRpc]
    public void SpawnTenisBallClientRpc(Vector3 spawn, Vector3 direction, ulong id)
    {
        if (NetworkManager.LocalClientId == id) return;

        var ammo = Instantiate(_ammoLocalPrefab);
        ammo.transform.position = spawn;
        var tenisBall = ammo.GetComponent<TenisBall>();

        tenisBall.Shoot(direction, spawn, _shotPower, id);
    }

    [ServerRpc]
    public void SpawnTenisBallServerRpc(Vector3 spawn, Vector3 direction, ulong id)
    {
        SpawnTenisBallClientRpc(spawn, direction,  id);
    }

    #endregion

    public IEnumerator CountCooldown()
    {
        yield return new WaitForSeconds(_cooldown);
        _setCooldown = false;
    }
}
