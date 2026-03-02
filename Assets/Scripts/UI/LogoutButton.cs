using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to any GameObject that has a Button component.
/// Assign the Button reference in the Inspector, or leave it empty to
/// auto-detect the Button on this GameObject.
///
/// Inspector references
/// ─────────────────────────────────────────────
///  logoutButton   → Button  (the logout button; auto-detected if empty)
///  loginScene     → string  (scene to load after logout, default "Login")
/// </summary>
[RequireComponent(typeof(Button))]
public class LogoutButton : MonoBehaviour
{
    [SerializeField] private Button logoutButton;

    /// <summary>Scene to load after logout.</summary>
    [SerializeField] private string loginScene = "Login";

    private void Awake()
    {
        if (logoutButton == null)
            logoutButton = GetComponent<Button>();

        logoutButton.onClick.AddListener(OnLogoutClicked);
    }

    private void OnDestroy()
    {
        logoutButton.onClick.RemoveListener(OnLogoutClicked);
    }

    private async void OnLogoutClicked()
    {
        logoutButton.interactable = false;

        try
        {
            if (AuthManager.Instance != null)
                await AuthManager.Instance.LogoutAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LogoutButton] Logout request failed: {e.Message}");
        }
        finally
        {
            // Always navigate to login, even if the server call failed
            SceneManager.LoadScene(loginScene);
        }
    }
}
