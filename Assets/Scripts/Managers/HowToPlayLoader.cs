using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayLoader : MonoBehaviour
{
    public void LoadHowToPlayScene()
    {
        FadeManager.Instance.LoadSceneWithFade("HowToPlayScene");

        AchievementManager.Instance.UnlockAchievement("first_time_player");
    }

}