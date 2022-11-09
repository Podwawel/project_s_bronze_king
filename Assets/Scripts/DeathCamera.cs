using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

public class DeathCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Canvas _deathCanvas;
    [SerializeField]
    private Canvas _commonCanvas;
    [SerializeField]
    private CanvasGroup _maskImage;
    [SerializeField]
    private CanvasGroup _maskText;
    [SerializeField]
    private CanvasGroup _loadingImage;

    public void OnDeath(Camera cam)
    {
        _commonCanvas.enabled = false;
        _deathCanvas.enabled = true;
        _maskImage.DOFade(1, 1f).OnComplete(() => {
            _camera.enabled = true;
            cam.enabled = false;
            _loadingImage.DOFade(1, 1f);
            _maskText.DOFade(1, 1f);
            _maskImage.DOFade(0, 1f);
        });
    }

    public void OnRespawn(Camera cam)
    {
        _maskText.DOFade(0, 1f);
        _loadingImage.DOFade(0, 1f);
        _maskImage.DOFade(1, 1f)
            .OnComplete(() => {
                _camera.enabled = false;
                cam.enabled = true;
                _commonCanvas.enabled = true;
                _maskImage.DOFade(0, 1f).OnComplete(() =>
                {
                    _deathCanvas.enabled = false;
                });
            });
    }
}
