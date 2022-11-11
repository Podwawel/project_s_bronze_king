using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TenisBall : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody _rigidbody;
    [SerializeField]
    private float _forceApplied;

    public float ForceApplied => _forceApplied;

    private Vector3 _forceVector;
    public Vector3 ForceVector => _forceVector;

    private ulong _ownerId;
    public ulong OwnerId => _ownerId;

    public void Shoot(Vector3 forceVector, Vector3 shotPoint, float power, ulong ownerId)
    {
        _ownerId = ownerId;
        _forceVector = forceVector;
        _rigidbody.AddForceAtPosition(forceVector * power, shotPoint, ForceMode.Impulse);
        _rigidbody.AddTorque(forceVector * power);
    }
}
