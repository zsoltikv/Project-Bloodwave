using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> availableItems = new List<ShopItem>();
    private List<ShopItem> currentShopItems = new List<ShopItem>();
    [SerializeField] private float refreshinterval = 60f;
    
    [Header("Events")]
    public UnityEvent<ShopItem> OnItemPurchased;
    public UnityEvent<string> OnPurchaseFailed;
    public UnityEvent OnShopRefreshed;

    [Header("UI")]
    public GameObject shopUI;
    [SerializeField] private GameObject pauseButton;

    private float nextRefreshTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        RefreshShop();
        nextRefreshTime = Time.time + refreshinterval;
    }

    void Update() {
        if (Time.time >= nextRefreshTime)
        {
            RefreshShop();
            nextRefreshTime = Time.time + refreshinterval;
        }
    }

    public void RefreshShop()
    {
        currentShopItems.Clear();

        List<ShopItem> shuffledItems = new List<ShopItem>(availableItems);

        int itemToSelect = Mathf.Min(3, shuffledItems.Count);

        for (int i = 0; i < itemToSelect; i++)
        {
            int randomIndex = Random.Range(0, shuffledItems.Count);
            currentShopItems.Add(shuffledItems[randomIndex]);
            shuffledItems.RemoveAt(randomIndex);
        }

        if (shopUI != null)
        {
            Transform item1 = shopUI.transform.GetChild(2);
            item1.GetComponent<Image>().sprite = currentShopItems[0].icon;
            item1.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentShopItems[0].itemName;
            item1.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentShopItems[0].description;
            item1.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Cost: " + currentShopItems[0].price;
            Button button1 = item1.GetComponent<Button>();
            button1.onClick.RemoveAllListeners(); 
            button1.onClick.AddListener(() => PurchaseItem(currentShopItems[0]));


            Transform item2 = shopUI.transform.GetChild(3);
            item2.GetComponent<Image>().sprite = currentShopItems[1].icon;
            item2.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentShopItems[1].itemName;
            item2.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentShopItems[1].description;
            item2.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Cost: " + currentShopItems[1].price;
            Button button2 = item2.GetComponent<Button>();
            button2.onClick.RemoveAllListeners(); 
            button2.onClick.AddListener(() => PurchaseItem(currentShopItems[1]));

            Transform item3 = shopUI.transform.GetChild(4);
            item3.GetComponent<Image>().sprite = currentShopItems[2].icon;
            item3.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentShopItems[2].itemName;
            item3.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentShopItems[2].description;
            item3.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Cost: " + currentShopItems[2].price;
            Button button3 = item3.GetComponent<Button>();
            button3.onClick.RemoveAllListeners();
            button3.onClick.AddListener(() => PurchaseItem(currentShopItems[2]));
        }
        
        OnShopRefreshed?.Invoke();
        Debug.Log("Shop refresh completed.");
    }


    public bool PurchaseItem(ShopItem item)
    {
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

        PlayerStats playerStats = PlayerInventory.instance.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            OnPurchaseFailed?.Invoke("Player stats not found");
            return false;
        }

        if (playerStats.Coins < item.price)
        {
            OnPurchaseFailed?.Invoke($"Not enough Coins! Need {item.price}, have {playerStats.Coins}");
            return false;
        }

        playerStats.Coins -= item.price;

        if (PlayerInventory.instance.AddItem(item))
        {
            OnItemPurchased?.Invoke(item);
            Debug.Log($"Purchased: {item.itemName} for {item.price} Coins");
            RefreshShop();
            return true;
        }
        else
        {
            playerStats.Coins += item.price;
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
        {
            availableItems.Add(item);
        }
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
        shopUI.SetActive(open);

        if (pauseButton != null)
            pauseButton.SetActive(!open);

        if (open)
        {
            GameManagerScript.instance.PauseGame();

            shopUI.transform.GetChild(1)
                .GetComponent<TMPro.TextMeshProUGUI>().text =
                $"Coins: {PlayerInventory.instance.GetComponent<PlayerStats>().Coins}";
        }
        else
        {
            GameManagerScript.instance.ResumeGame();
        }
    }

    public bool IsShopOpen()
    {
        return shopUI != null && shopUI.activeSelf;
    }

}
