using System;
using System.Collections;
using System.Net.Mail;
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
    [SerializeField] private float titleAnimationDuration = 0.4f;
    [SerializeField] private float panelAnimationDuration = 0.25f;
    [SerializeField] private string loginTitle = "Login";
    [SerializeField] private string registerTitle = "Register";

    /// <summary>Scene to load after a successful login.</summary>
    [SerializeField] private string mainMenuScene = "MainMenu";

    // ──────────────────────────────────────────────────────────────────────────

    private Coroutine loginButtonAnimationCoroutine;
    private Coroutine titleAnimationCoroutine;
    private Coroutine panelAnimationCoroutine;

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

        if (!IsValidEmail(email))
        {
            SetFeedback(registerFeedbackText, "Please enter a valid e-mail address.", error: true);
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
        // Deactivate the non-target panel immediately to avoid overlap
        if (loginPanel != null && loginPanel != target)
            loginPanel.SetActive(false);
        if (registerPanel != null && registerPanel != target)
            registerPanel.SetActive(false);

        // Ensure the target panel is active and animate its entrance
        if (target != null)
            StartPanelShowAnimation(target);

        // Update the auth title based on the active panel
        if (authTitleText != null)
        {
            if (target == loginPanel)
                StartTitleAnimation(loginTitle);
            else if (target == registerPanel)
                StartTitleAnimation(registerTitle);
        }
    }

    private void StartPanelShowAnimation(GameObject panel)
    {
        if (panel == null) return;

        if (panelAnimationCoroutine != null)
            StopCoroutine(panelAnimationCoroutine);
        panelAnimationCoroutine = StartCoroutine(AnimatePanelShow(panel));
    }

    private CanvasGroup EnsureCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator AnimatePanelShow(GameObject panel)
    {
        // Activate and prepare
        panel.SetActive(true);
        var cg = EnsureCanvasGroup(panel);
        var rect = panel.transform;
        var originalScale = rect.localScale;

        float duration = Mathf.Max(0.01f, panelAnimationDuration);
        float t = 0f;

        cg.alpha = 0f;
        rect.localScale = originalScale * 0.95f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / duration);
            // ease-out cubic
            float ease = 1f - Mathf.Pow(1f - f, 3f);
            cg.alpha = Mathf.Lerp(0f, 1f, ease);
            rect.localScale = Vector3.Lerp(originalScale * 0.95f, originalScale, ease);
            yield return null;
        }

        cg.alpha = 1f;
        rect.localScale = originalScale;
        panelAnimationCoroutine = null;
    }

    private void StartTitleAnimation(string newTitle)
    {
        if (authTitleText == null) return;
        if (authTitleText.text == newTitle) return;

        if (titleAnimationCoroutine != null)
            StopCoroutine(titleAnimationCoroutine);
        titleAnimationCoroutine = StartCoroutine(AnimateTitleChange(newTitle));
    }

    private IEnumerator AnimateTitleChange(string newTitle)
    {
        var tmp = authTitleText;
        var originalColor = tmp.color;
        var originalScale = tmp.transform.localScale;
        float duration = Mathf.Max(0.01f, titleAnimationDuration);
        float half = duration * 0.5f;

        // Fade out + scale down
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / half);
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - f);
            tmp.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, f);
            yield return null;
        }

        tmp.text = newTitle;

        // Fade in + scale up
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / half);
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, f);
            tmp.transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale, f);
            yield return null;
        }

        tmp.color = originalColor;
        tmp.transform.localScale = originalScale;
        titleAnimationCoroutine = null;
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

    private bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
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
