using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private bool _isLoading;
    private Coroutine _loadingRoutine;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeCanvas();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void StopAllTransitionCoroutines()
    {
        if (_loadingRoutine != null) StopCoroutine(_loadingRoutine);
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);

        _loadingRoutine = null;
        _fadeRoutine = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllTransitionCoroutines();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        _fadeRoutine = StartCoroutine(FadeIn());
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        Image image = panelGO.AddComponent<Image>();
        image.color = Color.black;

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        canvasGroup = panelGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (_isLoading) return;
        _isLoading = true;

        StopAllTransitionCoroutines();
        canvasGroup.blocksRaycasts = true;

        _loadingRoutine = StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return FadeOut();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;
    }

    public void ActivatePreloadedSceneWithFade(AsyncOperation preloadedOp)
    {
        if (_isLoading) return;
        if (preloadedOp == null) return;

        _isLoading = true;

        StopAllTransitionCoroutines();
        canvasGroup.blocksRaycasts = true;

        _loadingRoutine = StartCoroutine(FadeOutAndActivate(preloadedOp));
    }

    private IEnumerator FadeOutAndActivate(AsyncOperation preloadedOp)
    {
        yield return FadeOut();

        while (preloadedOp.progress < 0.9f)
            yield return null;

        preloadedOp.allowSceneActivation = true;

        while (!preloadedOp.isDone)
            yield return null;
    }

    private IEnumerator FadeOut()
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeIn()
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        _isLoading = false;
        _loadingRoutine = null;
        _fadeRoutine = null;
    }
}