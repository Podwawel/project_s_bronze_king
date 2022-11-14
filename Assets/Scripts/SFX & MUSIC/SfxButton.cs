using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SfxButton : MonoBehaviour
{
    [SerializeField]
    private GameObject _xIcon;
    [SerializeField]
    private Button _button;

    private bool _firstLaunch;

    private void Start()
    {
        _firstLaunch = true;
        OnClick();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_firstLaunch)
        {
            SoundManager.instance.EnableSFX(SoundManager.instance.SfxEnabled);
            _xIcon.SetActive(!SoundManager.instance.SfxEnabled);
            _firstLaunch = false;
            return;
        }

        SoundManager.instance.PlaySFX(SFX.BUTTON_CLICK, transform);

        SoundManager.instance.EnableSFX(!SoundManager.instance.SfxEnabled);
        _xIcon.SetActive(!SoundManager.instance.SfxEnabled);
    }
}
