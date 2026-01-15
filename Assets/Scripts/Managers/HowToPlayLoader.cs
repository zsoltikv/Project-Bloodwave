using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayLoader : MonoBehaviour
{

    public void LoadHowToPlayScene()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        FadeManager.Instance.LoadSceneWithFade("HowToPlayScene");
    }

}