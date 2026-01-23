using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{

    public void OnBack()
    {
        FadeManager.Instance.LoadSceneWithFade("MenuScene");
    }

}
