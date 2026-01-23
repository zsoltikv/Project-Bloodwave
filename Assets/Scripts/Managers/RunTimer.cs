using UnityEngine;
using TMPro;

public class RunTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    float timeElapsed = 0f;
    bool isRunning = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("GameStarted", 0) == 1)
        {
            isRunning = true;
            PlayerPrefs.SetInt("GameStarted", 0);
        }
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