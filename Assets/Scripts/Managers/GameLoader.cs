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

    public void Achievements()
    {
        FadeManager.Instance.LoadSceneWithFade("AchievementScene");
    }

    public void LoadHowToPlayScene()
    {
        FadeManager.Instance.LoadSceneWithFade("HowToPlayScene");
        AchievementManager.Instance.UnlockAchievement("first_time_player");
    }
}