using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{

    public void OnBack()
    {
        PlayerPrefs.SetInt("GameStarted", 1);
        FadeManager.Instance.LoadSceneWithFade("MenuScene");
    }

}
