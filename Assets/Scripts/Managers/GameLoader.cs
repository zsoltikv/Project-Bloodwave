using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    public void StartGame()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        FadeManager.Instance.LoadSceneWithFade("CutsceneScene");
    }

    public void LeaderBoard()
    {
        FadeManager.Instance.LoadSceneWithFade("LeaderBoardScene");
    }
}