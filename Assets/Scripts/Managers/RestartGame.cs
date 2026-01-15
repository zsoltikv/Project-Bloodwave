using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{

    public void GameRestart()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        FadeManager.Instance.LoadSceneWithFade("MainScene");
    }

}