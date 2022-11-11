using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotationVector;
    [SerializeField, Range(0, 100)]
    private int _rotationSpeed;

    private void Awake()
    {
        if (_rotationVector.magnitude == 0) return;

        transform.DORotate(_rotationVector, 0.01f * _rotationSpeed).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
}
