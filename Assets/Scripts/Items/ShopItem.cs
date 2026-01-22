using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public int price;

    [Header("Stat Modifiers")]
    public List<StatModifier> statModifiers = new List<StatModifier>();
    
    public bool isStackable = false;
    public int maxStackSize = 1;

    public void ApplyToPlayer(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            Debug.LogError($"PlayerStats is null! Cannot apply {itemName}");
            return;
        }

        Debug.Log($"Applying {itemName} to player. Modifiers count: {statModifiers.Count}");

        foreach (var modifier in statModifiers)
        {
            modifier.ApplyModifier(playerStats);
        }
        
        WeaponController weaponController = playerStats.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.RefreshAllOrbitingWeapons();
        }
    }

    public void RemoveFromPlayer(PlayerStats playerStats)
    {
        if (playerStats == null) return;

        foreach (var modifier in statModifiers)
        {
            modifier.RemoveModifier(playerStats);
        }
    }
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;

    public void ApplyModifier(PlayerStats playerStats)
    {
        Debug.Log($"Applying modifier: {statType} | Type: {modifierType} | Value: {value}");

        switch (statType)
        {
            case StatType.Health:
                float oldHealth = playerStats.MaxHealth;
                if (modifierType == ModifierType.Flat)
                    playerStats.MaxHealth += value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.MaxHealth *= (1 + value / 100f);
                Debug.Log($"Health changed: {oldHealth} -> {playerStats.MaxHealth}");
                playerStats.RefreshHpBar();
                playerStats.RefreshHpBar();
                break;

            case StatType.Damage:
                float oldDamage = playerStats.baseDamageMultiplier;
                if (modifierType == ModifierType.Flat)
                    playerStats.baseDamageMultiplier += value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseDamageMultiplier *= (1 + value / 100f);
                Debug.Log($"Damage multiplier changed: {oldDamage} -> {playerStats.baseDamageMultiplier}");
                break;

            case StatType.Cooldown:
                if (modifierType == ModifierType.Flat)
                    playerStats.CooldownMultiplier += value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.CooldownMultiplier += value / 100f;
                break;

            case StatType.Range:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseRangeMultiplier += value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseRangeMultiplier *= (1 + value / 100f);
                break;

            case StatType.ProjectileSpeed:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseProjectileSpeed += value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseProjectileSpeed *= (1 + value / 100f);
                break;

            case StatType.ProjectileCount:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseProjectileBonus += (int)value;
                break;
        }
    }

    public void RemoveModifier(PlayerStats playerStats)
    {
        switch (statType)
        {
            case StatType.Health:
                if (modifierType == ModifierType.Flat)
                    playerStats.Health -= value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.Health /= (1 + value / 100f);
                playerStats.RefreshHpBar();
                break;

            case StatType.Damage:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseDamageMultiplier -= value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseDamageMultiplier /= (1 + value / 100f);
                break;

            case StatType.Cooldown:
                if (modifierType == ModifierType.Flat)
                    playerStats.CooldownMultiplier -= value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.CooldownMultiplier -= value / 100f;
                break;

            case StatType.Range:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseRangeMultiplier -= value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseRangeMultiplier /= (1 + value / 100f);
                break;

            case StatType.ProjectileSpeed:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseProjectileSpeed -= value;
                else if (modifierType == ModifierType.Percentage)
                    playerStats.baseProjectileSpeed /= (1 + value / 100f);
                break;

            case StatType.ProjectileCount:
                if (modifierType == ModifierType.Flat)
                    playerStats.baseProjectileBonus -= (int)value;
                break;
        }
    }
}

public enum StatType
{
    Health,
    Damage,
    Cooldown,
    Range,
    ProjectileSpeed,
    ProjectileCount,
    asd
}

public enum ModifierType
{
    Flat,           
    Percentage    
}
