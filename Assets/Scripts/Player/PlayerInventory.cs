using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [Header("Inventory")]
    [SerializeField] private List<InventorySlot> items = new List<InventorySlot>();
    public UnityEvent<ShopItem> OnItemAdded;
    public UnityEvent<ShopItem> OnItemRemoved;

    private PlayerStats playerStats;

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

        playerStats = GetComponent<PlayerStats>();
    }

    // Item kezelés
    public bool AddItem(ShopItem item)
    {
        if (item == null) return false;

        if (item.isStackable)
        {
            InventorySlot existingSlot = items.Find(slot => slot.item == item);
            
            if (existingSlot != null && existingSlot.quantity < item.maxStackSize)
            {
                existingSlot.quantity++;
                item.ApplyToPlayer(playerStats);
                OnItemAdded?.Invoke(item);
                return true;
            }
            else if (existingSlot == null)
            {
                items.Add(new InventorySlot(item, 1));
                item.ApplyToPlayer(playerStats);
                OnItemAdded?.Invoke(item);
                return true;
            }
            
            return false; // Stack tele
        }
        else
        {
            // Nem stackelhető item
            items.Add(new InventorySlot(item, 1));
            item.ApplyToPlayer(playerStats);
            OnItemAdded?.Invoke(item);
            return true;
        }
    }

    public bool RemoveItem(ShopItem item, int quantity = 1)
    {
        if (item == null) return false;

        InventorySlot slot = items.Find(s => s.item == item);
        
        if (slot != null)
        {
            // Először eltávolítjuk a hatást
            for (int i = 0; i < quantity; i++)
            {
                item.RemoveFromPlayer(playerStats);
            }

            slot.quantity -= quantity;
            
            if (slot.quantity <= 0)
            {
                items.Remove(slot);
            }
            
            OnItemRemoved?.Invoke(item);
            return true;
        }
        
        return false;
    }

    public bool HasItem(ShopItem item)
    {
        return items.Exists(slot => slot.item == item);
    }

    public int GetItemQuantity(ShopItem item)
    {
        InventorySlot slot = items.Find(s => s.item == item);
        return slot?.quantity ?? 0;

    }
}

[System.Serializable]
public class InventorySlot
{
    public ShopItem item;
    public int quantity;

    public InventorySlot(ShopItem item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}
