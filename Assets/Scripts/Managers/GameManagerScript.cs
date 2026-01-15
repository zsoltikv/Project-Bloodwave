using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    public bool FreezeGame = false;
    
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


}
