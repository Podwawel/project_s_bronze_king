using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField]
    private Animator _animator;

    public void SetTrigger(Animation anim)
    {
        switch (anim)
        {
            case Animation.JUMP:
                _animator.SetTrigger("Jump");
                break;
            case Animation.DANCE:
                _animator.SetTrigger("Dance");
                break;
            case Animation.SHOOT:
                _animator.SetTrigger("Shoot");
                break;
        }
    }

    public void SetBool(Animation anim, bool active)
    {
        switch (anim)
        {
            case Animation.RUN:
                _animator.SetBool("Run", active);
                break;
            case Animation.IDLE:
                _animator.SetBool("Idle", active);
                break;
            case Animation.CHARGE:
                _animator.SetBool("Charge", active);
                break;
        }
    }
}

public enum Animation
{
    DANCE,
    RUN,
    IDLE,
    SHOOT,
    CHARGE,
    JUMP,
}
