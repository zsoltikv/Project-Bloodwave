using UnityEngine;

public enum UpgradeType
{
    Damage,
    ProjectileCount,
    Cooldown,
    Range
}

[System.Serializable]
public class WeaponUpgrade
{
    public WeaponInstance targetWeapon;
    public UpgradeType upgradeType;
    public float value;
    
    [System.NonSerialized]
    public WeaponController weaponController;

    public string GetDescription()
    {
        string weaponName = targetWeapon.definition.weaponName;
        
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                return $"{weaponName}: +{value} Damage";
            case UpgradeType.ProjectileCount:
                return $"{weaponName}: +{(int)value} Projectiles";
            case UpgradeType.Cooldown:
                return $"{weaponName}: -{value * 100:F0}% Cooldown";
            case UpgradeType.Range:
                return $"{weaponName}: +{value * 100:F0}% Range";
            default:
                return "Unknown Upgrade";
        }
    }

    public void Apply()
    {
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                targetWeapon.bonusDamage += value;
                break;
            case UpgradeType.ProjectileCount:
                targetWeapon.bonusProjectileCount += (int)value;
                // Refresh orbiting weapons when projectile count changes
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
            case UpgradeType.Cooldown:
                targetWeapon.cooldownMultiplier *= (1f - value);
                break;
            case UpgradeType.Range:
                targetWeapon.rangeMultiplier *= (1f + value);
                // Refresh orbiting weapons when range changes
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
        }
        
        targetWeapon.level++;
    }
}
