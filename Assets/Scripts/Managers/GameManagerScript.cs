using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    public bool FreezeGame = false;
    public bool saveUsedThisRun = false;
    public int level;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PauseGame()
    {
        FreezeGame = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        FreezeGame = false;
        Time.timeScale = 1f;
    }

    public void PauseResumeGame()
    {
        if (FreezeGame)
            ResumeGame();
        else
            PauseGame();
    }

    public void GetLevel(int _level)
    {
        level = _level;
    }
}