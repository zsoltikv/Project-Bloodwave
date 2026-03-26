using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthUIHandler : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField loginUsernameField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private Toggle rememberMeToggle;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button switchToRegisterBtn;
    [SerializeField] private TMP_Text loginFeedbackText;

    [Header("Register Panel")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private TMP_InputField regUsernameField;
    [SerializeField] private TMP_InputField regEmailField;
    [SerializeField] private TMP_InputField regPasswordField;
    [SerializeField] private TMP_InputField regConfirmPasswordField;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button switchToLoginBtn;
    [SerializeField] private TMP_Text registerFeedbackText;

    [Header("Misc")]
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TMP_Text authTitleText;
    [SerializeField] private string loginTitle = "Login";
    [SerializeField] private string registerTitle = "Register";

    /// <summary>Scene to load after a successful login.</summary>
    [SerializeField] private string mainMenuScene = "MainMenu";

    // ──────────────────────────────────────────────────────────────────────────

    private Coroutine loginButtonAnimationCoroutine;

    private void Start()
    {
        // Subscribe to the session-expired event so any screen in the game can
        // redirect back here when a token refresh fails.
        AuthManager.OnSessionExpired += HandleSessionExpired;

        // Wire buttons (avoids cluttering the Inspector with persistent listeners)
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        switchToRegisterBtn.onClick.AddListener(() => ShowPanel(registerPanel));
        switchToLoginBtn.onClick.AddListener(() => ShowPanel(loginPanel));

        // Ensure the "Remember Me" toggle is off by default
        if (rememberMeToggle != null)
            rememberMeToggle.isOn = false;

        // If already logged in, skip straight to the game
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn())
        {
            var savedUser = AuthManager.Instance.GetUser();
            Debug.Log($"[Auth] Auto-login from saved token – username: '{savedUser?.username ?? "unknown"}'");
            SceneManager.LoadScene(mainMenuScene);
            return;
        }

        ShowPanel(loginPanel);
    }

    private void OnDestroy()
    {
        AuthManager.OnSessionExpired -= HandleSessionExpired;

        loginButton.onClick.RemoveListener(OnLoginClicked);
        registerButton.onClick.RemoveListener(OnRegisterClicked);
    }

    // ─── Button handlers ───────────────────────────────────────────────────────

    /// <summary>Called by the Login button's OnClick event (or wired above).</summary>
    private async void OnLoginClicked()
    {
        var username = loginUsernameField.text.Trim();
        var password = loginPasswordField.text;
        var rememberMe = rememberMeToggle != null && rememberMeToggle.isOn;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetFeedback(loginFeedbackText, "Please fill in all fields.", error: true);
            return;
        }

        SetLoading(true);
        SetFeedback(loginFeedbackText, string.Empty);

        // Start the login button animation
        if (loginButtonAnimationCoroutine != null)
            StopCoroutine(loginButtonAnimationCoroutine);
        loginButtonAnimationCoroutine = StartCoroutine(AnimateLoginButton());

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
            // Stop the animation and restore original button text
            if (loginButtonAnimationCoroutine != null)
            {
                StopCoroutine(loginButtonAnimationCoroutine);
                loginButtonAnimationCoroutine = null;
            }
            loginButton.GetComponentInChildren<TMP_Text>().text = "Login";
        }
    }

    /// <summary>Called by the Register button's OnClick event (or wired above).</summary>
    private async void OnRegisterClicked()
    {
        var username = regUsernameField.text.Trim();
        var email = regEmailField.text.Trim();
        var password = regPasswordField.text;
        var confirmPassword = regConfirmPasswordField != null ? regConfirmPasswordField.text : password;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            SetFeedback(registerFeedbackText, "Please fill in all fields.", error: true);
            return;
        }

        if (password != confirmPassword)
        {
            SetFeedback(registerFeedbackText, "Passwords do not match.", error: true);
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
        loginPanel.SetActive(loginPanel == target);
        registerPanel.SetActive(registerPanel == target);
        // Update the auth title based on the active panel
        if (authTitleText != null)
        {
            if (target == loginPanel)
                authTitleText.text = loginTitle;
            else if (target == registerPanel)
                authTitleText.text = registerTitle;
        }
    }

    private void SetFeedback(TMP_Text label, string message, bool error = false)
    {
        if (label == null) return;
        label.text = message;
        label.color = error ? Color.red : Color.green;
    }

    private void SetLoading(bool active)
    {
        if (loadingOverlay != null)
            loadingOverlay.SetActive(active);

        loginButton.interactable = !active;
        registerButton.interactable = !active;
    }

    private IEnumerator AnimateLoginButton()
    {
        var buttonText = loginButton.GetComponentInChildren<TMP_Text>();
        string[] frames = { "Logging in.", "Logging in..", "Logging in..." };
        int frameIndex = 0;

        while (true)
        {
            buttonText.text = frames[frameIndex];
            frameIndex = (frameIndex + 1) % frames.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
