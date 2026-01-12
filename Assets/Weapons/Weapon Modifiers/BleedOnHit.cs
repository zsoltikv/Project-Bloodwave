using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedOnHit", menuName = "Weapon Modifiers/Bleed On Hit")]
public class BleedOnHit : WeaponModifier
{
    public float bleedRatio = 8f;
    public float bleedDuration = 2f;

    public override void OnHit(ref WeaponContext context, HitInfo hit)
    {

        float bleedDamage = hit.damage / bleedRatio;
        var targetStats = hit.target.GetComponent<EnemyHealth>();
        if (targetStats != null)
        {
            targetStats.ApplyBleed(bleedDamage, bleedDuration);
        }
    }
}