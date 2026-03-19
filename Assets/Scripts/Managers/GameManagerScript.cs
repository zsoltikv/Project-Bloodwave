using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    public bool FreezeGame = false;
    public bool saveUsedThisRun = false;
    public int level;

    public int damageDealtThisRun = 0;
    public int coinsCollectedThisRun = 0;
    public int enemiesKilledThisRun = 0;

    public void ResetRunStats()
    {
        damageDealtThisRun = 0;
        coinsCollectedThisRun = 0;
        enemiesKilledThisRun = 0;
    }

    public void AddDamageDealt(int amount)
    {
        if (amount <= 0) return;
        damageDealtThisRun += amount;
    }

    public void AddCoinsCollected(int amount)
    {
        if (amount <= 0) return;
        coinsCollectedThisRun += amount;
    }

    public void AddEnemyKilled()
    {
        enemiesKilledThisRun++;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ResetRunStats();
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