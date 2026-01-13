using Unity.VisualScripting;
using UnityEngine;

public class LevelUpScript : MonoBehaviour
{

    [Header("Options")]
    public GameObject FirstOption;
    public GameObject SecondOption;


    public GameObject DismissButton;

    void OnEnable()
    {
        
    }

    public void DismissLevelUp()
    {
        GameManagerScript.instance.ResumeGame();
        this.gameObject.SetActive(false);
    }
}
