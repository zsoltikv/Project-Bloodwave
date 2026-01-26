using System.Collections;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject shopButton;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;

    private CanvasGroup pauseCanvasGroup;
    private Vector3 originalScale;
    private Coroutine animRoutine;
    private bool isPaused;

    void Awake()
    {
        if (pauseUI == null) return;

        pauseCanvasGroup = pauseUI.GetComponent<CanvasGroup>();
        originalScale = pauseUI.transform.localScale;

        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
        pauseUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseUI();
        }
    }

    public void TogglePauseUI()
    {
        if (pauseUI == null) return;

        if (ShopManager.instance != null && ShopManager.instance.IsShopOpen())
            return;

        isPaused = !isPaused;

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        if (isPaused)
            animRoutine = StartCoroutine(OpenPauseAnim());
        else
            animRoutine = StartCoroutine(ClosePauseAnim());

        Time.timeScale = isPaused ? 0f : 1f;

        if (shopButton != null)
            shopButton.SetActive(!isPaused);
    }

    private IEnumerator OpenPauseAnim()
    {
        pauseUI.SetActive(true);

        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;

        pauseUI.transform.localScale = originalScale * 0.9f;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / animDuration;

            pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, lerp);
            pauseUI.transform.localScale =
                Vector3.Lerp(originalScale * 0.9f, originalScale, EaseOutBack(lerp));

            yield return null;
        }

        pauseCanvasGroup.alpha = 1f;
        pauseUI.transform.localScale = originalScale;
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator ClosePauseAnim()
    {
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / animDuration;

            pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, lerp);
            pauseUI.transform.localScale =
                Vector3.Lerp(originalScale, originalScale * 0.9f, lerp);

            yield return null;
        }

        pauseCanvasGroup.alpha = 0f;
        pauseUI.transform.localScale = originalScale;
        pauseUI.SetActive(false);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}