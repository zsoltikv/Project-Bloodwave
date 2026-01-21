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
            // Első item (child 1)
            Transform item1 = shopUI.transform.GetChild(1);
            item1.GetComponentInChildren<TextMeshProUGUI>().text = currentShopItems[0].itemName + " - " + currentShopItems[0].price + " Coins";
            Button button1 = item1.GetComponent<Button>();
            button1.onClick.RemoveAllListeners(); 
            button1.onClick.AddListener(() => PurchaseItem(currentShopItems[0]));

            // Második item (child 2)
            Transform item2 = shopUI.transform.GetChild(2);
            item2.GetComponentInChildren<TextMeshProUGUI>().text = currentShopItems[1].itemName + " - " + currentShopItems[1].price + " Coins";
            Button button2 = item2.GetComponent<Button>();
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(() => PurchaseItem(currentShopItems[1]));

            // Harmadik item (child 3)
            Transform item3 = shopUI.transform.GetChild(3);
            item3.GetComponentInChildren<TextMeshProUGUI>().text = currentShopItems[2].itemName + " - " + currentShopItems[2].price + " Coins";
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

        // Szerezzük meg a PlayerStats-t a pénzhez
        PlayerStats playerStats = PlayerInventory.instance.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            OnPurchaseFailed?.Invoke("Player stats not found");
            return false;
        }

        // Ellenőrizzük van-e elég pénz
        if (playerStats.Coins < item.price)
        {
            OnPurchaseFailed?.Invoke($"Not enough Coins! Need {item.price}, have {playerStats.Coins}");
            return false;
        }

        // Elköltsük a pénzt
        playerStats.Coins -= item.price;

        // Hozzáadjuk az inventoryhoz
        if (PlayerInventory.instance.AddItem(item))
        {
            OnItemPurchased?.Invoke(item);
            Debug.Log($"Purchased: {item.itemName} for {item.price} Coins");
            RefreshShop();
            return true;
        }
        else
        {
            // Visszaadjuk a pénzt ha nem sikerült hozzáadni
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
        if (shopUI != null)
        {
            shopUI.SetActive(!shopUI.activeSelf);
        }
    }
}
