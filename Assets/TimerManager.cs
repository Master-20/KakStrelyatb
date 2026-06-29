using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    [Header("Настройка таймера")]
    public float startTimeSeconds = 600f;

    [Header("UI Elements")]
    public Text timerText;
    public GameObject messagePanel;

    private float remainingTime;
    private bool isRunning = false;

    private void Start()
    {
        remainingTime = startTimeSeconds;
        if (messagePanel != null)
            messagePanel.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(true);
        isRunning = true;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            ShowEndMessage();
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void ShowEndMessage()
    {
        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (messagePanel != null)
            messagePanel.SetActive(true);
    }
}
