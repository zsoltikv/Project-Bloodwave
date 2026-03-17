using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    [SerializeField] private string loginScene = "AuthScene";

    public async void Logout()
    {
        try
        {
            if (AuthManager.Instance != null)
            {
                await AuthManager.Instance.LogoutAsync();
            }
            else
            {
                ClearLocalAuthKeys();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LogoutManager] Logout request failed: {e.Message}");
            ClearLocalAuthKeys();
        }
        finally
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(loginScene);
            }
            else
            {
                SceneManager.LoadScene(loginScene);
            }
        }
    }

    private void ClearLocalAuthKeys()
    {
        PlayerPrefs.DeleteKey("bw_token");
        PlayerPrefs.DeleteKey("bw_refreshToken");
        PlayerPrefs.DeleteKey("bw_expiresAt");
        PlayerPrefs.DeleteKey("bw_user");
        PlayerPrefs.DeleteKey("bw_remember");
        PlayerPrefs.Save();
    }
}
