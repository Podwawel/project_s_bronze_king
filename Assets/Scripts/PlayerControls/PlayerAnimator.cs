using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField]
    private Transform _playerSpine;
    [SerializeField]
    private Animator _animator;
    public void LeanBodyWithCamera(float rot)
    {
        LeanBodyServerRpc(rot);
        LeanBodyClientRpc(rot);
    }

    [ServerRpc]
    public void LeanBodyServerRpc(float rot)
    {
        _playerSpine.transform.localRotation = Quaternion.Euler(-rot, 0f, 0f);
    }
    [ClientRpc]
    public void LeanBodyClientRpc(float rot)
    {
        _playerSpine.transform.localRotation = Quaternion.Euler(-rot, 0f, 0f);
    }

    public void SetTrigger(Animation anim)
    {
        SetTriggerClientRpc(anim);
        SetTriggerServerRpc(anim);
    }

    public void SetBool(Animation anim, bool active)
    {
        SetBoolClientRpc(anim, active);
        SetBoolServerRpc(anim, active);
    }


    [ServerRpc]
    public void SetTriggerServerRpc(Animation anim)
    {
        switch (anim)
        {
            case Animation.JUMP:
                _animator.SetTrigger("Jump");
                break;
            case Animation.VICTORY:
                _animator.SetTrigger("Victory");
                break;
            case Animation.DANCE:
                _animator.SetTrigger("Dance");
                break;
        }
    }

    [ClientRpc]
    public void SetTriggerClientRpc(Animation anim)
    {
        switch (anim)
        {
            case Animation.JUMP:
                _animator.SetTrigger("Jump");
                break;
            case Animation.VICTORY:
                _animator.SetTrigger("Victory");
                break;
            case Animation.DANCE:
                _animator.SetTrigger("Dance");
                break;
        }
    }

    [ServerRpc]
    public void SetBoolServerRpc(Animation anim, bool active)
    {
        switch (anim)
        {
            case Animation.RUN:
                _animator.SetBool("Run", active);
                break;
            case Animation.IDLE:
                _animator.SetBool("Idle", active);
                break;
        }
    }
    [ClientRpc]
    public void SetBoolClientRpc(Animation anim, bool active)
    {
        switch (anim)
        {
            case Animation.RUN:
                _animator.SetBool("Run", active);
                break;
            case Animation.IDLE:
                _animator.SetBool("Idle", active);
                break;
        }
    }
}

public enum Animation
{
    DANCE,
    VICTORY,
    RUN,
    IDLE,
    JUMP,
}
