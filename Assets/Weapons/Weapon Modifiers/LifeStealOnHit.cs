using UnityEngine;

[CreateAssetMenu(fileName = "LifeStealOnHit", menuName = "Weapon Modifiers/Life Steal On Hit")]
public class LifeStealOnHit : WeaponModifier
{
    [Range(0f, 1f)]
    public float lifeStealPercent = 0.1f; // 10% of damage dealt

    private PlayerStats cachedPlayerStats;

    public override void OnHit(ref WeaponContext context, HitInfo hit)
    {
        float healAmount = hit.damage * lifeStealPercent;
        
        if (cachedPlayerStats == null)
        {
            cachedPlayerStats = Object.FindAnyObjectByType<PlayerStats>();
        }
        
        if (cachedPlayerStats != null)
        {
            cachedPlayerStats.Heal(healAmount);
        }
    }
}
