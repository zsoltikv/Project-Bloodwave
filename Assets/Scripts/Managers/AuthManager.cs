using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// ─── Data models ───────────────────────────────────────────────────────────────

[Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[Serializable]
public class RegisterRequest
{
    public string username;
    public string email;
    public string password;
}

[Serializable]
public class RefreshRequest
{
    public string refreshToken;
    public string expiresAt;
}

[Serializable]
public class UserData
{
    public string id;
    public string username;
    public string email;
}

[Serializable]
public class AuthResponse
{
    public bool   success;
    public string message;
    public string token;
    public string refreshToken;
    public string expiresAt;
    public UserData user;
}

[Serializable]
public class MatchCreateRequest
{
    public int time;
    public int level;
    public int maxHealth;
    public int damageDealt;
    public int enemiesKilled;
    public int coinsCollected;
    public System.Collections.Generic.List<int> itemIds = new System.Collections.Generic.List<int>();
    public System.Collections.Generic.List<int> weaponIds = new System.Collections.Generic.List<int>();
}

// ─── AuthManager ───────────────────────────────────────────────────────────────

/// <summary>
/// Singleton that mirrors the JS auth layer (cookies / sessionStorage → PlayerPrefs / memory).
/// Place on a persistent GameObject in your first scene.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    // ── Config ─────────────────────────────────────────────────────────────────
    private const string API_BASE          = "http://5.38.140.128:5000";
    private const int    REFRESH_BUFFER_SEC = 60;

    // ── PlayerPrefs keys (persistent storage, analogous to cookies) ────────────
    private const string KEY_TOKEN         = "bw_token";
    private const string KEY_REFRESH_TOKEN = "bw_refreshToken";
    private const string KEY_EXPIRES_AT    = "bw_expiresAt";
    private const string KEY_USER          = "bw_user";
    private const string KEY_REMEMBER      = "bw_remember";

    // ── In-memory session (analogous to sessionStorage, cleared on app quit) ───
    private string _sessionToken;
    private string _sessionRefreshToken;
    private string _sessionExpiresAt;
    private string _sessionUser;

    /// <summary>
    /// Subscribe to this to handle "session expired → go to login screen" navigation.
    /// E.g.: AuthManager.OnSessionExpired += () => SceneManager.LoadScene("Login");
    /// </summary>
    public static event Action OnSessionExpired;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Session storage helpers ───────────────────────────────────────────────

    /// <summary>
    /// Saves the session after a successful login / refresh.
    /// rememberMe = true  → PlayerPrefs (survives app restart, mirrors persistent cookies).
    /// rememberMe = false → in-memory only (cleared when the app closes, mirrors sessionStorage).
    /// </summary>
    private void SaveSession(AuthResponse data, bool rememberMe = true)
    {
        if (rememberMe)
        {
            // Clear in-memory so the persistent values are always read
            _sessionToken = _sessionRefreshToken = _sessionExpiresAt = _sessionUser = null;

            PlayerPrefs.SetString(KEY_TOKEN,         data.token);
            PlayerPrefs.SetString(KEY_REFRESH_TOKEN, data.refreshToken);
            PlayerPrefs.SetString(KEY_EXPIRES_AT,    data.expiresAt);
            PlayerPrefs.SetString(KEY_USER,          JsonUtility.ToJson(data.user));
            PlayerPrefs.SetInt   (KEY_REMEMBER,      1);
            PlayerPrefs.Save();
        }
        else
        {
            ClearPlayerPrefs();

            _sessionToken        = data.token;
            _sessionRefreshToken = data.refreshToken;
            _sessionExpiresAt    = data.expiresAt;
            _sessionUser         = data.user != null ? JsonUtility.ToJson(data.user) : null;
        }
    }

    public void ClearSession()
    {
        _sessionToken = _sessionRefreshToken = _sessionExpiresAt = _sessionUser = null;
        ClearPlayerPrefs();
    }

    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(KEY_TOKEN);
        PlayerPrefs.DeleteKey(KEY_REFRESH_TOKEN);
        PlayerPrefs.DeleteKey(KEY_EXPIRES_AT);
        PlayerPrefs.DeleteKey(KEY_USER);
        PlayerPrefs.DeleteKey(KEY_REMEMBER);
        PlayerPrefs.Save();
    }

    // In-memory takes precedence (rememberMe=false path); falls back to PlayerPrefs
    private string Get(string key)
    {
        string mem = key switch
        {
            KEY_TOKEN         => _sessionToken,
            KEY_REFRESH_TOKEN => _sessionRefreshToken,
            KEY_EXPIRES_AT    => _sessionExpiresAt,
            KEY_USER          => _sessionUser,
            _                 => null
        };
        if (!string.IsNullOrEmpty(mem)) return mem;
        string pref = PlayerPrefs.GetString(key, null);
        return string.IsNullOrEmpty(pref) ? null : pref;
    }

    // ─── Public accessors ──────────────────────────────────────────────────────

    public string   GetToken()        => Get(KEY_TOKEN);
    public string   GetRefreshToken() => Get(KEY_REFRESH_TOKEN);
    public string   GetExpiresAt()    => Get(KEY_EXPIRES_AT);

    public UserData GetUser()
    {
        var raw = Get(KEY_USER);
        if (string.IsNullOrEmpty(raw)) return null;
        try   { return JsonUtility.FromJson<UserData>(raw); }
        catch { return null; }
    }

    /// <summary>True when any session data exists.</summary>
    public bool IsLoggedIn() =>
        !string.IsNullOrEmpty(GetToken()) || !string.IsNullOrEmpty(GetRefreshToken());

    private bool IsTokenExpired()
    {
        var expiresAt = GetExpiresAt();
        if (string.IsNullOrEmpty(expiresAt)) return true;

        if (!DateTime.TryParse(expiresAt, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
            return true;

        return DateTime.UtcNow >= expiry.ToUniversalTime().AddSeconds(-REFRESH_BUFFER_SEC);
    }

    // ─── Token refresh ─────────────────────────────────────────────────────────

    public async Task<AuthResponse> RefreshSessionAsync()
    {
        var refreshToken = GetRefreshToken();
        var expiresAt    = GetExpiresAt();
        if (string.IsNullOrEmpty(refreshToken))
            throw new Exception("No refresh token");

        var body = JsonUtility.ToJson(new RefreshRequest
        {
            refreshToken = refreshToken,
            expiresAt    = expiresAt
        });

        AuthResponse data;
        try
        {
            data = await PostAsync<AuthResponse>("/api/refresh/update", body, authenticated: false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AuthManager] Token refresh FAILED: {e.Message}");
            throw;
        }

        bool rememberMe = PlayerPrefs.HasKey(KEY_REMEMBER);
        SaveSession(data, rememberMe);
        Debug.Log($"[AuthManager] Token refreshed successfully – username: '{data.user?.username ?? "unknown"}'");
        return data;
    }

    /// <summary>
    /// Ensures a valid access token is available.
    /// Silently refreshes if expired; fires OnSessionExpired and throws if refresh fails.
    /// </summary>
    public async Task EnsureValidTokenAsync()
    {
        if (!IsTokenExpired()) return;

        if (string.IsNullOrEmpty(GetRefreshToken()))
        {
            ClearSession();
            OnSessionExpired?.Invoke();
            throw new Exception("Not authenticated");
        }

        try
        {
            await RefreshSessionAsync();
        }
        catch
        {
            ClearSession();
            OnSessionExpired?.Invoke();
            throw new Exception("Session expired. Please log in again.");
        }
    }

    // ─── Auth endpoints ────────────────────────────────────────────────────────

    /// <summary>Register a new account. Does NOT auto-login.</summary>
    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        var body = JsonUtility.ToJson(new RegisterRequest
        {
            username = username,
            email    = email,
            password = password
        });
        return await PostAsync<AuthResponse>("/api/user", body, authenticated: false);
    }

    /// <summary>Login and persist the session.</summary>
    public async Task<AuthResponse> LoginAsync(string username, string password, bool rememberMe = false)
    {
        var body = JsonUtility.ToJson(new LoginRequest
        {
            username = username,
            password = password
        });
        AuthResponse data;
        try
        {
            data = await PostAsync<AuthResponse>("/api/user/login", body, authenticated: false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AuthManager] Login FAILED for '{username}': {e.Message}");
            throw;
        }

        SaveSession(data, rememberMe);
        Debug.Log($"[AuthManager] Login successful – username: '{data.user?.username ?? username}', rememberMe: {rememberMe}");
        return data;
    }

    /// <summary>Logout, clear local session, fire OnSessionExpired.</summary>
    public async Task LogoutAsync()
    {
        try
        {
            var token = GetToken();
            using var req = new UnityWebRequest($"{API_BASE}/api/user/logout", "POST");
            req.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");
            await SendAsync(req);
        }
        finally
        {
            ClearSession();
            OnSessionExpired?.Invoke();
        }
    }

    public async Task<string> CreateMatchAsync(MatchCreateRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        string body = JsonUtility.ToJson(request);
        return await AuthFetchAsync("/api/Match", "POST", body);
    }

    // ─── Authenticated fetch wrapper ───────────────────────────────────────────

    /// <summary>
    /// Drop-in equivalent of authFetch().
    /// Proactively refreshes the token, then attaches it.
    /// Falls back to one retry on an unexpected 401.
    /// Returns the raw response body as a string — deserialise with JsonUtility as needed.
    /// </summary>
    public async Task<string> AuthFetchAsync(string endpoint, string method = "GET", string jsonBody = null)
    {
        await EnsureValidTokenAsync();

        var res = await SendRequestAsync(endpoint, method, jsonBody, GetToken());

        if (res.responseCode == 401)
        {
            try
            {
                await RefreshSessionAsync();
                res = await SendRequestAsync(endpoint, method, jsonBody, GetToken());
            }
            catch
            {
                ClearSession();
                OnSessionExpired?.Invoke();
                throw new Exception("Session expired. Please log in again.");
            }

            if (res.responseCode == 401)
            {
                ClearSession();
                OnSessionExpired?.Invoke();
                throw new Exception("Session expired. Please log in again.");
            }
        }

        return res.downloadHandler.text;
    }

    // ─── Low-level HTTP helpers ────────────────────────────────────────────────

    private async Task<T> PostAsync<T>(string endpoint, string json, bool authenticated = true)
    {
        string token = authenticated ? GetToken() : null;
        var res = await SendRequestAsync(endpoint, "POST", json, token);

        if (res.result == UnityWebRequest.Result.ConnectionError ||
            res.result == UnityWebRequest.Result.DataProcessingError)
            throw new Exception($"Network error: {res.error}");

        var data = JsonUtility.FromJson<AuthResponse>(res.downloadHandler.text);

        if (res.responseCode < 200 || res.responseCode >= 300)
            throw new Exception(data?.message ?? $"Request failed ({res.responseCode})");

        // Re-deserialise into the concrete type (works fine when T == AuthResponse)
        return JsonUtility.FromJson<T>(res.downloadHandler.text);
    }

    private Task<UnityWebRequest> SendRequestAsync(
        string endpoint, string method, string body, string token)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var req  = new UnityWebRequest($"{API_BASE}{endpoint}", method);

        if (!string.IsNullOrEmpty(body))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.SetRequestHeader("Content-Type", "application/json");
        }

        req.downloadHandler = new DownloadHandlerBuffer();

        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", $"Bearer {token}");

        var op = req.SendWebRequest();
        op.completed += _ => tcs.SetResult(req);
        return tcs.Task;
    }

    private Task<UnityWebRequest> SendAsync(UnityWebRequest req)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var op  = req.SendWebRequest();
        op.completed += _ => tcs.SetResult(req);
        return tcs.Task;
    }
}
