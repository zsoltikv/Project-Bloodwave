using System.Collections;
using UnityEngine;
using TMPro;

public class WelcomeUser : MonoBehaviour
{
    [SerializeField] private float characterDelay = 0.04f;

    void Awake()
    {
        var userInfo = AuthManager.Instance?.GetUser();
        var welcomeText = GetComponent<TMP_Text>();
        string message;

        if (userInfo != null && !string.IsNullOrEmpty(userInfo.username))
        {
            message = $"Welcome back, {userInfo.username}!";
        }
        else
        {
            message = "Welcome back, Dev!";
        }

        if (welcomeText != null)
        {
            StartCoroutine(Typewriter(welcomeText, message));
        }
        else
        {
            Debug.Log(message);
        }
    }

    private IEnumerator Typewriter(TMP_Text targetText, string fullMessage)
    {
        targetText.text = fullMessage;
        targetText.maxVisibleCharacters = 0;
        targetText.ForceMeshUpdate();

        int totalCharacters = targetText.textInfo.characterCount;
        float delay = Mathf.Max(0f, characterDelay);

        for (int i = 1; i <= totalCharacters; i++)
        {
            targetText.maxVisibleCharacters = i;

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
        }
    }
}
