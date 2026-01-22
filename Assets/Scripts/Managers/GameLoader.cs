using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    public void StartGame()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        FadeManager.Instance.LoadSceneWithFade("MainScene");
    }

    public void LeaderBoard()
    {
        FadeManager.Instance.LoadSceneWithFade("LeaderBoardScene");
    }
}