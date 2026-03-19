using UnityEngine;

public class OpenWebPageButton : MonoBehaviour
{
    [SerializeField] private string url = "https://www.google.com";

    public void OnClickOpenWebPage()
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("[OpenWebPageButton] URL is empty.");
            return;
        }

        Application.OpenURL(url);
    }
}