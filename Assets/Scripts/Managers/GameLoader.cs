using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    public void StartGame()
    {
        FadeManager.Instance.LoadSceneWithFade("CutsceneScene");

    }

    public void LeaderBoard()
    {
        FadeManager.Instance.LoadSceneWithFade("LeaderBoardScene");
    }
}