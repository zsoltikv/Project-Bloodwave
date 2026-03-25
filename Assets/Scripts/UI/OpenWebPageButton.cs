using UnityEngine;

public class OpenWebPageButton : MonoBehaviour
{
    [SerializeField] private string url = "http://bloodwave.site/";

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