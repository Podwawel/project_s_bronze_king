using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using DG.Tweening;

public class RespawnManager : Singleton<RespawnManager>
{
    [SerializeField]
    private TextMeshProUGUI _countText;
    [SerializeField]
    private Image _loadingImage;
    public Vector3 GetRespawnPoint()
    {
        return new Vector3(0, 20, 0);
    }

    public IEnumerator StartCounting(int time)
    {
        _loadingImage.transform.DORotate(new Vector3(0, 0, 360 * 3), time + 2, RotateMode.FastBeyond360).SetEase(Ease.Linear);

        for(int i = time; i > 0; i--)
        {
            _countText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }
}
