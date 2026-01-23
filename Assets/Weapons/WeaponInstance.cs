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
    public float GetDamage() => Mathf.Max(definition.Damage * bonusDamage * playerStats.baseDamageMultiplier, 0F);
    public int GetProjectileCount() => Mathf.Max((definition.ProjectileCount + bonusProjectileCount) + playerStats.baseProjectileBonus, 1);
    public float GetCooldown() => Mathf.Max(definition.Cooldown * cooldownMultiplier * (1 - playerStats.CooldownMultiplier), 0F);
    public float GetRange() => Mathf.Max(definition.baseRange * rangeMultiplier * playerStats.baseRangeMultiplier, 0.5F);
    public float GetProjectileSpeed() => Mathf.Max(definition.ProjectileSpeed * playerStats.baseProjectileSpeed, 1F);
    public float GetOrbitalSpeed() => Mathf.Max(orbitalSpeedMultiplier * playerStats.baseProjectileSpeed, 1F);
}
