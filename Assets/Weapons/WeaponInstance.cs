using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponDefinition definition;
    public int level = 1;
    public float cooldownTimer;
    [System.NonSerialized] public bool isFiring = false;

    [Header("Upgrade Bonuses")]
    public float bonusDamage = 0f;
    public int bonusProjectileCount = 0;
    public float cooldownMultiplier = 1f;
    public float rangeMultiplier = 1f;

    public WeaponInstance(WeaponDefinition definition)
    {
        this.definition = definition;
    }
    public float GetDamage() => definition.Damage + bonusDamage;
    public int GetProjectileCount() => definition.ProjectileCount + bonusProjectileCount;
    public float GetCooldown() => definition.Cooldown * cooldownMultiplier;
    public float GetRange() => definition.baseRange * rangeMultiplier;
}
