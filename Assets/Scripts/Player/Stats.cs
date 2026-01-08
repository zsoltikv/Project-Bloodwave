using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Base")]
    public float baseDamageMultiplier = 1f;
    public float baseCooldownMultiplier = 1f; // 1 = normál, 0.8 = gyorsabb
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;

    [Header("Runtime buffs (optional)")]
    public float damageBonusMultiplier = 0f;       // +0.2 = +20%
    public float cooldownBonusMultiplier = 0f;     // -0.2 = 20%-kal gyorsabb (ha így kezeled)
    public float rangeBonusMultiplier = 0f;
    public float projectileSpeedBonus = 0f;
    public int projectileBonus = 0;

    // Kényelmes property-k:
    public float DamageMultiplier => baseDamageMultiplier * (1f + damageBonusMultiplier);

    // Cooldown: itt úgy számolom, hogy minél kisebb, annál gyorsabb.
    // Ha inkább "attack speed" jellegűt akarsz, szólj és átalakítom.
    public float CooldownMultiplier => Mathf.Max(0.05f, baseCooldownMultiplier * (1f + cooldownBonusMultiplier));

    public float RangeMultiplier => baseRangeMultiplier * (1f + rangeBonusMultiplier);
    public float ProjectileSpeed => Mathf.Max(0.1f, baseProjectileSpeed + projectileSpeedBonus);
    public int ProjectileBonus => baseProjectileBonus + projectileBonus;
}
