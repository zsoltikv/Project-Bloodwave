using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{

    public void GameRestart()
    {
        GameManagerScript.instance.ResumeGame();
        RunTimer.instance.ResetTimer();
        FadeManager.Instance.LoadSceneWithFade("MainScene");

    }

}