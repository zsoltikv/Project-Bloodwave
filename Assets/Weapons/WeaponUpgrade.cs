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
        if (targetWeapon == null || targetWeapon.definition == null)
            return;

        switch (upgradeType)
        {
            case UpgradeType.Damage:
                targetWeapon.bonusDamage += value;
                break;

            case UpgradeType.ProjectileCount:
                targetWeapon.bonusProjectileCount += (int)value;
                break;

            case UpgradeType.Cooldown:
                targetWeapon.cooldownMultiplier *= (1f - value);
                targetWeapon.cooldownMultiplier = Mathf.Clamp(targetWeapon.cooldownMultiplier, 0.05f, 10f);
                break;

            case UpgradeType.Range:
                targetWeapon.rangeMultiplier *= (1f + value);
                break;

            case UpgradeType.OrbitalSpeed:
                targetWeapon.orbitalSpeedMultiplier *= (1f + value);
                break;
        }

        targetWeapon.level++;

        if (weaponController != null)
            weaponController.RefreshAllOrbitingWeapons();

        var am = AchievementManager.Instance;
        if (am == null) return;

        am.UnlockAchievement("first_weapon_upgrade");

        switch (upgradeType)
        {
            case UpgradeType.Damage: am.UnlockAchievement("upgrade_damage_once"); break;
            case UpgradeType.ProjectileCount: am.UnlockAchievement("upgrade_projectiles_once"); break;
            case UpgradeType.Cooldown: am.UnlockAchievement("upgrade_cooldown_once"); break;
            case UpgradeType.Range: am.UnlockAchievement("upgrade_range_once"); break;
            case UpgradeType.OrbitalSpeed: am.UnlockAchievement("upgrade_orbitalspeed_once"); break;
        }

        if (targetWeapon.level >= 5) am.UnlockAchievement("weapon_level_5");
        if (targetWeapon.level >= 10) am.UnlockAchievement("weapon_level_10");

        if (targetWeapon.bonusProjectileCount >= 3) am.UnlockAchievement("projectiles_bonus_3");
        if (targetWeapon.cooldownMultiplier <= 0.5f) am.UnlockAchievement("cooldown_50");
        if (targetWeapon.rangeMultiplier >= 1.5f) am.UnlockAchievement("range_150");
        if (targetWeapon.orbitalSpeedMultiplier >= 2.0f) am.UnlockAchievement("orbitalspeed_200");
    }
}
