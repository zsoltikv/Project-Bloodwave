using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base")]
    public float baseDamageMultiplier = 1f;
    public float baseCooldownMultiplier = 1f; // 1 = normál, 0.8 = gyorsabb
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;

    [Header("Runtime buffs (optional)")]
    public float damageBonusMultiplier = 0f;       // +0.2 = +20%
    public float cooldownBonusMultiplier = 0f;     // -0.2 = 20%-kal gyorsabb
    public float rangeBonusMultiplier = 0f;
    public float projectileSpeedBonus = 0f;
    public int projectileBonus = 0;

    public float DamageMultiplier => baseDamageMultiplier * (1f + damageBonusMultiplier);

    public float CooldownMultiplier => Mathf.Max(0.05f, baseCooldownMultiplier * (1f + cooldownBonusMultiplier));

    public float RangeMultiplier => baseRangeMultiplier * (1f + rangeBonusMultiplier);
    public float ProjectileSpeed => Mathf.Max(0.1f, baseProjectileSpeed + projectileSpeedBonus);
    public int ProjectileBonus => baseProjectileBonus + projectileBonus;

    public int XP { get; private set; } = 0;
    public int Coins { get; private set; } = 0;

    public void AddXP(int amount)
    {
        XP += amount;
        Debug.Log($"XP gained: {amount}. Total XP: {XP}");
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log($"Coins gained: {amount}. Total Coins: {Coins}");
    }
}