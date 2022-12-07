using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerPhysics : NetworkBehaviour
{
    public event Action OnDeathEvent;
    public event Action OnWinEvent;

    [SerializeField]
    private Rigidbody _rigidbody;

    [Space, SerializeField]
    private float _slopeForceRayLength;
    [SerializeField]
    private float _slopeForce;

    [Space, SerializeField]
    private Transform _groundCheck;
    [SerializeField]
    private float _distanceToGround;
    [SerializeField]
    private LayerMask _groundMask;

    public void MoveRigidbody(Vector3 vectorValue)
    {
        _rigidbody.MovePosition(_rigidbody.position + vectorValue);
    }

    public bool IsGrounded()
    {
        var state = Physics.CheckSphere(_groundCheck.position, _distanceToGround, _groundMask);
        return state;
    }

    private bool SlopeCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)  return true;
        }

        return false;
    }

    public void SetKinematic(bool active)
    {
        _rigidbody.isKinematic = active;
    }

    public void OnSlope()
    {
        if (!SlopeCheck()) return;

        _rigidbody.MovePosition(_rigidbody.position + (Vector3.down * _slopeForce * 0.05f * Time.fixedDeltaTime));
    }

    public void AddForceImpulse(Vector3 forceVector)
    {
        _rigidbody.AddForce(forceVector, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            var ball = collision.gameObject.GetComponent<TenisBall>();
            if (NetworkManager.LocalClientId == ball.OwnerId) return;
            ImpactPlayerByBallServerRpc(ball.ForceVector * ball.ForceApplied * Time.fixedDeltaTime * 30);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ImpactPlayerByBallServerRpc(Vector3 forceVector)
    {
        ImpactPlayerByBallClientRpc(forceVector);
    }

    [ClientRpc]
    public void ImpactPlayerByBallClientRpc(Vector3 forceVector)
    {
        AddForceImpulse(forceVector);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            WaterSplashSoundServerRpc();
        }

        if (other.gameObject.layer == 9)
        {
            other.gameObject.SetActive(false);
        }

        if (!IsOwner) return;

        if (other.gameObject.layer == 6)
        {
            OnDeathEvent?.Invoke();
        }
        if (other.gameObject.layer == 9)
        {
            OnWinEvent?.Invoke();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void WaterSplashSoundServerRpc()
    {
        WaterSplashSoundClientRpc();
    }

    [ClientRpc]
    public void WaterSplashSoundClientRpc()
    {
        SoundManager.instance.PlaySFX(SFX.WATER_LOSE, transform);
    }

    public void StopRigidbody()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        _rigidbody.velocity = newVelocity;
    }

    public Vector3 GetVelocity()
    {
        return _rigidbody.velocity;
    }
}