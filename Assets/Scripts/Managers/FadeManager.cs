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
    private Coroutine _currentTransition;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ne avatkozzunk bele, ha épp transition van folyamatban
        if (_isLoading) return;

        // Biztonsági fallback: ha valahogy mégis betöltõdött scene transition nélkül
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIn());
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

        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
        }

        _currentTransition = StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        _isLoading = true;
        canvasGroup.blocksRaycasts = true;

        // Fade out
        yield return FadeOut();

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        // Fade in
        yield return FadeIn();

        // Cleanup
        _isLoading = false;
        _currentTransition = null;
    }

    public void ActivatePreloadedSceneWithFade(AsyncOperation preloadedOp)
    {
        if (_isLoading) return;
        if (preloadedOp == null) return;

        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
        }

        _currentTransition = StartCoroutine(TransitionToPreloadedScene(preloadedOp));
    }

    private IEnumerator TransitionToPreloadedScene(AsyncOperation preloadedOp)
    {
        _isLoading = true;
        canvasGroup.blocksRaycasts = true;

        // Fade out
        yield return FadeOut();

        // Wait for preloaded scene to be ready
        while (preloadedOp.progress < 0.9f)
            yield return null;

        // Activate scene
        preloadedOp.allowSceneActivation = true;

        while (!preloadedOp.isDone)
            yield return null;

        // Fade in
        yield return FadeIn();

        // Cleanup
        _isLoading = false;
        _currentTransition = null;
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
    }
}