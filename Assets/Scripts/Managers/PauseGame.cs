using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseUI;

    private bool isPaused;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseUI();
        }
    }

    public void TogglePauseUI()
    {
        if (pauseUI == null) return;

        isPaused = !isPaused;

        pauseUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }
}