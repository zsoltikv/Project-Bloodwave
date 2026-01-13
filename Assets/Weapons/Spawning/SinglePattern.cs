using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Spawn/Single")]
public class SinglePattern : SpawnPattern
{
    public override IEnumerable<Shot> BuildShots(WeaponContext ctx, targetInfo target)
    {
        Vector3 dir = target.hasTarget ? target.direction : ctx.firePoint.right;

        yield return new Shot
        {
            position = ctx.firePoint.position,
            direction = dir,
            damage = ctx.weapon.definition.Damage,
            speed  = ctx.stats.baseProjectileSpeed,
            range  = ctx.weapon.definition.baseRange
        };
    }
}
