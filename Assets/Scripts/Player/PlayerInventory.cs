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

    private void Awake()
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

    public bool AddItem(ShopItem item)
    {
        if (item == null) return false;

        if (item.weaponDefinition != null)
        {
            playerStats.GetComponent<WeaponController>().AddWeapon(item.weaponDefinition);
            OnItemAdded?.Invoke(item);
            return true;
        }

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

            if (existingSlot == null)
            {
                items.Add(new InventorySlot(item, 1));
                item.ApplyToPlayer(playerStats);
                OnItemAdded?.Invoke(item);
                return true;
            }

            return false;
        }

        items.Add(new InventorySlot(item, 1));
        item.ApplyToPlayer(playerStats);
        OnItemAdded?.Invoke(item);

        return true;
    }

    public bool RemoveItem(ShopItem item, int quantity = 1)
    {
        if (item == null) return false;

        InventorySlot slot = items.Find(s => s.item == item);

        if (slot != null)
        {
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

    public List<int> GetOwnedItemIds()
    {
        List<int> itemIds = new List<int>();

        foreach (InventorySlot slot in items)
        {
            if (slot == null || slot.item == null) continue;

            int id = slot.item.ItemId;
            if (id <= 0) continue;

            int quantity = Mathf.Max(1, slot.quantity);
            for (int i = 0; i < quantity; i++)
            {
                itemIds.Add(id);
            }
        }

        return itemIds;
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