using UnityEngine;

public class SetFPS : MonoBehaviour
{
    private static bool isInitialized = false;

    void Awake()
    {
        if (isInitialized == false)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            isInitialized = true;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}