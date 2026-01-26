using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject shopButton;

    private bool isPaused;

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

        pauseUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (shopButton != null)
            shopButton.SetActive(!isPaused);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}