using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> availableItems = new List<ShopItem>();
    [SerializeField] private List<WeaponDefinition> availableWeapons = new List<WeaponDefinition>(); 
    private readonly List<ShopItem> currentShopItems = new List<ShopItem>();

    [Header("Events")]
    public UnityEvent<ShopItem> OnItemPurchased;
    public UnityEvent<string> OnPurchaseFailed;
    public UnityEvent OnShopRefreshed;

    [Header("UI")]
    public GameObject shopUI;
    private TextMeshProUGUI coinDisplay;
    [SerializeField] private float itemBoughtDuration = 2f;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject shopButton;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 0.8f);

    private CanvasGroup shopCanvasGroup;
    private Coroutine animRoutine;
    private Vector3 originalScale;

    private int totalPurchases = 0;
    private readonly HashSet<string> purchasedItemIds = new HashSet<string>();

    private const string TotalSpentKey = "TotalCoinsSpent";
    private int totalCoinsSpent = 0;
    private bool bigSpenderUnlocked = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        if (shopUI == null)
        {
            Debug.LogError("ShopManager: shopUI nincs be�ll�tva!");
            enabled = false;
            return;
        }

        coinDisplay = shopUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        shopCanvasGroup = shopUI.GetComponent<CanvasGroup>();
        if (shopCanvasGroup == null) shopCanvasGroup = shopUI.AddComponent<CanvasGroup>();

        originalScale = shopUI.transform.localScale;

        totalCoinsSpent = PlayerPrefs.GetInt(TotalSpentKey, 0);
        bigSpenderUnlocked = AchievementManager.Instance.IsAchievementUnlocked("big_spender");
    }

    void Start()
    {
        BeginNewRun();
    }
    public void BeginNewRun()
    {
        purchasedItemIds.Clear();
        totalPurchases = 0;
        RefreshShop();
    }

    public void RefreshShop()
    {
        WeaponController weaponController = PlayerInventory.instance.GetComponent<WeaponController>();
        List<WeaponDefinition> ownedWeapons = weaponController.GetAllWeapons().ConvertAll(w => w.definition);

        currentShopItems.Clear();  
        var filteredItems = availableItems.Where(item =>
            item != null &&
            !IsPurchasedThisRun(item) &&
            (item.weaponDefinition == null || !ownedWeapons.Contains(item.weaponDefinition))
        ).ToList();

        /*if ( weaponController.GetWeapons().Count == 3 )
        {
            filteredItems = filteredItems.Where(item => item.weaponDefinition == null).ToList();
        }*/

        int itemToSelect = Mathf.Min(3, filteredItems.Count);

        if (itemToSelect == 0)
        {
            Debug.Log("No items left to show in shop (this run).");

            SetSlotActive(2, false);
            SetSlotActive(3, false);
            SetSlotActive(4, false);

            OnShopRefreshed?.Invoke();
            return;
        }

        List<ShopItem> pool = new List<ShopItem>(filteredItems);
        for (int i = 0; i < itemToSelect; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            currentShopItems.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        int[] slotIndices = { 2, 3, 4 };
        for (int i = 0; i < slotIndices.Length; i++)
        {
            Transform slot = shopUI.transform.GetChild(slotIndices[i]);
            bool hasItem = i < currentShopItems.Count;

            slot.gameObject.SetActive(hasItem);
            if (!hasItem) continue;

            ShopItem it = currentShopItems[i];

            slot.GetComponent<Image>().sprite = it.icon;
            slot.GetChild(0).GetComponent<TextMeshProUGUI>().text = it.itemName;
            slot.GetChild(1).GetComponent<TextMeshProUGUI>().text = it.description;
            slot.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Cost: " + it.price;

            Button btn = slot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            ShopItem captured = it;
            btn.onClick.AddListener(() => PurchaseItem(captured));
        }

        OnShopRefreshed?.Invoke();
        Debug.Log("Shop refresh completed.");
    }

    public void RefreshShopButton()
    {
        PlayerStats playerStats = PlayerInventory.instance.GetComponent<PlayerStats>();

        if (playerStats.Coins >= 100)
        {
            playerStats.Coins -= 100;
            coinDisplay.text = $"Coins: {playerStats.Coins}";
            RefreshShop();
        }
    }

    public bool PurchaseItem(ShopItem item)
    {
        AchievementManager.Instance.UnlockAchievement("shopaholic");

        if (item == null)
        {
            OnPurchaseFailed?.Invoke("Invalid item");
            return false;
        }

        if (!currentShopItems.Contains(item))
        {
            OnPurchaseFailed?.Invoke("Item not available in current shop");
            return false;
        }

        if (IsPurchasedThisRun(item))
        {
            OnPurchaseFailed?.Invoke("Already purchased this run");
            return false;
        }

        PlayerStats playerStats = PlayerInventory.instance.GetComponent<PlayerStats>();

        if (playerStats.Coins < item.price)
        {
            OnPurchaseFailed?.Invoke($"Not enough Coins! Need {item.price}, have {playerStats.Coins}");
            return false;
        }

        playerStats.Coins -= item.price;

        if (PlayerInventory.instance.AddItem(item))
        {
            coinDisplay.text = $"Coins: {playerStats.Coins}";
            OnItemPurchased?.Invoke(item);

            AddLifetimeSpent(item.price);

            totalPurchases++;
            if (totalPurchases == 10)
            {
                AchievementManager.Instance.UnlockAchievement("collector");
                AchievementManager.Instance.UnlockAchievement("shop_clear_10");
            }

            MarkPurchasedThisRun(item);

            RefreshShop();
            return true;
        }
        else
        {
            playerStats.Coins += item.price;
            coinDisplay.text = $"Coins: {playerStats.Coins}";
            OnPurchaseFailed?.Invoke("Inventory full or item stack limit reached");
            return false;
        }
    }

    public bool CanAfford(ShopItem item)
    {
        if (item == null) return false;

        PlayerStats playerStats = PlayerInventory.instance?.GetComponent<PlayerStats>();
        return playerStats != null && playerStats.Coins >= item.price;
    }

    public List<ShopItem> GetAvailableItems()
    {
        return new List<ShopItem>(availableItems);
    }

    public void AddItemToShop(ShopItem item)
    {
        if (item != null && !availableItems.Contains(item))
            availableItems.Add(item);
    }

    public void RemoveItemFromShop(ShopItem item)
    {
        availableItems.Remove(item);
    }

    public void ToggleShopUI()
    {
        if (shopUI == null) return;

        PauseGame pause = FindObjectOfType<PauseGame>();
        if (pause != null && pause.IsPaused())
            return;

        bool open = !shopUI.activeSelf;

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = open ? StartCoroutine(OpenShopAnim()) : StartCoroutine(CloseShopAnim());

        if (pauseButton != null)
            pauseButton.SetActive(!open);

        coinDisplay.text = $"Coins: {PlayerInventory.instance.GetComponent<PlayerStats>().Coins}";

        if (open) GameManagerScript.instance.PauseGame();
        else GameManagerScript.instance.ResumeGame();
    }

    private IEnumerator OpenShopAnim()
    {
        shopUI.SetActive(true);

        shopCanvasGroup.alpha = 0f;
        shopCanvasGroup.interactable = false;
        shopCanvasGroup.blocksRaycasts = false;

        shopUI.transform.localScale = originalScale * 0.9f;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / animDuration;

            shopCanvasGroup.alpha = Mathf.Lerp(0f, 1f, lerp);
            shopUI.transform.localScale =
                Vector3.Lerp(originalScale * 0.9f, originalScale, EaseOutBack(lerp));

            yield return null;
        }

        shopCanvasGroup.alpha = 1f;
        shopUI.transform.localScale = originalScale;
        shopCanvasGroup.interactable = true;
        shopCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator CloseShopAnim()
    {
        shopCanvasGroup.interactable = false;
        shopCanvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / animDuration;

            shopCanvasGroup.alpha = Mathf.Lerp(1f, 0f, lerp);
            shopUI.transform.localScale =
                Vector3.Lerp(originalScale, originalScale * 0.9f, lerp);

            yield return null;
        }

        shopCanvasGroup.alpha = 0f;
        shopUI.transform.localScale = originalScale;
        shopUI.SetActive(false);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }

    public bool IsShopOpen()
    {
        return shopUI != null && shopUI.activeSelf;
    }

    public void DisableShopUI()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        if (shopUI != null)
            shopUI.SetActive(false);

        if (shopButton != null)
            shopButton.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (shopCanvasGroup != null)
        {
            shopCanvasGroup.interactable = false;
            shopCanvasGroup.blocksRaycasts = false;
        }
    }
    private void AddLifetimeSpent(int amount)
    {
        if (amount <= 0) return;

        totalCoinsSpent += amount;
        PlayerPrefs.SetInt(TotalSpentKey, totalCoinsSpent);
        PlayerPrefs.Save();

        if (!bigSpenderUnlocked && totalCoinsSpent >= 5000)
        {
            bigSpenderUnlocked = true;
            AchievementManager.Instance.UnlockAchievement("big_spender");
        }
    }
    private string GetItemId(ShopItem item)
    {
        return item != null ? item.name : "";
    }

    private bool IsPurchasedThisRun(ShopItem item)
    {
        return purchasedItemIds.Contains(GetItemId(item));
    }

    private void MarkPurchasedThisRun(ShopItem item)
    {
        purchasedItemIds.Add(GetItemId(item));
    }

    private void SetSlotActive(int childIndex, bool active)
    {
        if (shopUI == null) return;
        if (childIndex < 0 || childIndex >= shopUI.transform.childCount) return;
        shopUI.transform.GetChild(childIndex).gameObject.SetActive(active);
    }
}