using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameTimer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timeText;
    [SerializeField]
    private CanvasGroup _timerPanel;

    private int _minutes;
    public int Minutes => _minutes;
    private int _seconds;
    public int Seconds => _seconds;

    private float _timeOverall;

    private bool _count;

    private void Awake()
    {
        _count = true;
    }

    private void Update()
    {
        if (!_count) return;

        _timeOverall += Time.deltaTime;
        _minutes = (int)_timeOverall / 60;
        _seconds = (int)_timeOverall % 60;
        string seconds = _seconds < 10 ? "0" + _seconds : $"{_seconds}";
        string timeText = _minutes + ":" + seconds;
        _timeText.text = timeText;
    }

    public void ResetTimer()
    {
        _timeOverall = 0f;
        _timeText.text = "0:00";
    }

    public void StopTimer()
    {
        _timerPanel.DOFade(0, 1f);
        _count = false;
    }
}
