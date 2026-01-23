using UnityEngine;
using TMPro;

public class RunTimer : MonoBehaviour
{
    TextMeshProUGUI timerText;
    float timeElapsed = 0f;
    bool isRunning = false;

    void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        timeElapsed += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}