using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to a Canvas that contains Login and Register panels.
/// Wire every field and button reference in the Inspector (see comments).
///
/// SCENE REFERENCES TO SET IN INSPECTOR
/// ──────────────────────────────────────────────────────────────────────
///  Login panel
///    loginPanel          → the Login Panel GameObject
///    loginUsernameField  → TMP_InputField  "Username"
///    loginPasswordField  → TMP_InputField  "Password"
///    rememberMeToggle    → Toggle          "Remember me"
///    loginButton         → Button          "Log In"
///    switchToRegisterBtn → Button          "Don't have an account? Register"
///    loginFeedbackText   → TMP_Text        (error / status label)
///
///  Register panel
///    registerPanel          → the Register Panel GameObject
///    regUsernameField       → TMP_InputField  "Username"
///    regEmailField          → TMP_InputField  "Email"
///    regPasswordField       → TMP_InputField  "Password"
///    registerButton         → Button          "Create Account"
///    switchToLoginBtn       → Button          "Already have an account? Log In"
///    registerFeedbackText   → TMP_Text        (error / status label)
///
///  (Optional) loadingOverlay → a full-screen dimming panel shown while requests are in flight
/// ──────────────────────────────────────────────────────────────────────
/// </summary>
public class AuthUIHandler : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject    loginPanel;
    [SerializeField] private TMP_InputField loginUsernameField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private Toggle         rememberMeToggle;
    [SerializeField] private Button         loginButton;
    [SerializeField] private Button         switchToRegisterBtn;
    [SerializeField] private TMP_Text       loginFeedbackText;

    [Header("Register Panel")]
    [SerializeField] private GameObject     registerPanel;
    [SerializeField] private TMP_InputField regUsernameField;
    [SerializeField] private TMP_InputField regEmailField;
    [SerializeField] private TMP_InputField regPasswordField;
    [SerializeField] private Button         registerButton;
    [SerializeField] private Button         switchToLoginBtn;
    [SerializeField] private TMP_Text       registerFeedbackText;

    [Header("Misc")]
    [SerializeField] private GameObject loadingOverlay;

    /// <summary>Scene to load after a successful login.</summary>
    [SerializeField] private string mainMenuScene = "MainMenu";

    // ──────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Subscribe to the session-expired event so any screen in the game can
        // redirect back here when a token refresh fails.
        AuthManager.OnSessionExpired += HandleSessionExpired;

        // Wire buttons (avoids cluttering the Inspector with persistent listeners)
        loginButton        .onClick.AddListener(OnLoginClicked);
        registerButton     .onClick.AddListener(OnRegisterClicked);
        switchToRegisterBtn.onClick.AddListener(() => ShowPanel(registerPanel));
        switchToLoginBtn   .onClick.AddListener(() => ShowPanel(loginPanel));

        // If already logged in, skip straight to the game
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn())
        {
            SceneManager.LoadScene(mainMenuScene);
            return;
        }

        ShowPanel(loginPanel);
    }

    private void OnDestroy()
    {
        AuthManager.OnSessionExpired -= HandleSessionExpired;

        loginButton        .onClick.RemoveListener(OnLoginClicked);
        registerButton     .onClick.RemoveListener(OnRegisterClicked);
    }

    // ─── Button handlers ───────────────────────────────────────────────────────

    /// <summary>Called by the Login button's OnClick event (or wired above).</summary>
    private async void OnLoginClicked()
    {
        var username   = loginUsernameField.text.Trim();
        var password   = loginPasswordField.text;
        var rememberMe = rememberMeToggle != null && rememberMeToggle.isOn;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetFeedback(loginFeedbackText, "Please fill in all fields.", error: true);
            return;
        }

        SetLoading(true);
        SetFeedback(loginFeedbackText, string.Empty);

        try
        {
            await AuthManager.Instance.LoginAsync(username, password, rememberMe);
            SceneManager.LoadScene(mainMenuScene);
        }
        catch (Exception e)
        {
            SetFeedback(loginFeedbackText, e.Message, error: true);
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>Called by the Register button's OnClick event (or wired above).</summary>
    private async void OnRegisterClicked()
    {
        var username = regUsernameField.text.Trim();
        var email    = regEmailField   .text.Trim();
        var password = regPasswordField.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email)    ||
            string.IsNullOrEmpty(password))
        {
            SetFeedback(registerFeedbackText, "Please fill in all fields.", error: true);
            return;
        }

        SetLoading(true);
        SetFeedback(registerFeedbackText, string.Empty);

        try
        {
            await AuthManager.Instance.RegisterAsync(username, email, password);

            // Registration succeeded — show a success message then switch to login
            SetFeedback(registerFeedbackText, "Account created! Please log in.", error: false);
            await System.Threading.Tasks.Task.Delay(1500);
            ShowPanel(loginPanel);
        }
        catch (Exception e)
        {
            SetFeedback(registerFeedbackText, e.Message, error: true);
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ─── Session expiry ────────────────────────────────────────────────────────

    private void HandleSessionExpired()
    {
        // Called from any scene when the refresh token is gone or invalid
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ─── UI helpers ────────────────────────────────────────────────────────────

    private void ShowPanel(GameObject target)
    {
        loginPanel   .SetActive(loginPanel    == target);
        registerPanel.SetActive(registerPanel == target);
    }

    private void SetFeedback(TMP_Text label, string message, bool error = false)
    {
        if (label == null) return;
        label.text  = message;
        label.color = error ? Color.red : Color.green;
    }

    private void SetLoading(bool active)
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(active);

        loginButton   .interactable = !active;
        registerButton.interactable = !active;
    }
}
