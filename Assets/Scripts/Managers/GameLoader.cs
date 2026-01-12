using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    public void StartGame()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        SceneManager.LoadScene("MainScene");
    }

}