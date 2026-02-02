using UnityEngine;
using TMPro;

public class RunTimer : MonoBehaviour
{
    public static RunTimer instance;
    public GameObject timerText;
    public float timeElapsed = 0f;
    bool isRunning = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;
        if (timerText == null)
        {
            timerText = GameObject.Find("RunTimerText");
            if (timerText == null) return;
        }

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= 300f)
        {
            AchievementManager.Instance.UnlockAchievement("survivor_5min");
        }

        DisplayTimer(timeElapsed);
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        timeElapsed = 0f;
    }

    public void DisplayTimer(float timeElapsed)
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        timerText.GetComponent<TextMeshProUGUI>().text = $"{minutes:00}:{seconds:00}";
    }
}