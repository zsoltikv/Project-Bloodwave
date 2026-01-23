using UnityEngine;

public enum UpgradeType
{
    Damage,
    ProjectileCount,
    Cooldown,
    Range,
    OrbitalSpeed
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
                return $"{weaponName}: +{value * 100:F0}% Damage";
            case UpgradeType.ProjectileCount:
                return $"{weaponName}: +{(int)value} Projectiles";
            case UpgradeType.Cooldown:
                return $"{weaponName}: -{value * 100:F0}% Cooldown";
            case UpgradeType.Range:
                return $"{weaponName}: +{value * 100:F0}% Range";
            case UpgradeType.OrbitalSpeed:
                return $"{weaponName}: +{value * 100:F0}% Orbital Speed";
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
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
            case UpgradeType.ProjectileCount:
                targetWeapon.bonusProjectileCount += (int)value;
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
            case UpgradeType.Cooldown:
                targetWeapon.cooldownMultiplier *= (1f - value);
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
            case UpgradeType.Range:
                targetWeapon.rangeMultiplier *= (1f + value);
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
            case UpgradeType.OrbitalSpeed:
                targetWeapon.orbitalSpeedMultiplier *= (1f + value);
                if (weaponController != null)
                    weaponController.RefreshAllOrbitingWeapons();
                break;
        }
        
        targetWeapon.level++;
    }
}
