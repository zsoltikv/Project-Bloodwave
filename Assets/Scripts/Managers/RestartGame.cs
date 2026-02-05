using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void GameRestart()
    {
        FadeManager.Instance.LoadSceneWithFade("MainScene");

        AchievementManager.Instance.UnlockAchievement("first_restart");
    }
}