using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponDefinition definition;
    public int level = 1;
    public float cooldownTimer;
    [System.NonSerialized] public bool isFiring = false;
    [System.NonSerialized] public PlayerStats playerStats;

    [Header("Upgrade Bonuses")]
    public float bonusDamage = 1f;
    public int bonusProjectileCount = 0;
    public float cooldownMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public float orbitalSpeedMultiplier = 1f;

    public WeaponInstance(WeaponDefinition definition)
    {
        this.definition = definition;
    }
    public float GetDamage() => definition.Damage * bonusDamage * playerStats.baseDamageMultiplier;
    public int GetProjectileCount() => (definition.ProjectileCount + bonusProjectileCount) + playerStats.baseProjectileBonus;
    public float GetCooldown() => definition.Cooldown * cooldownMultiplier * (1 - playerStats.CooldownMultiplier);
    public float GetRange() => definition.baseRange * rangeMultiplier * playerStats.baseRangeMultiplier;
    public float GetProjectileSpeed() => definition.ProjectileSpeed * playerStats.baseProjectileSpeed;
    public float GetOrbitalSpeed() => orbitalSpeedMultiplier * playerStats.baseProjectileSpeed;
}
