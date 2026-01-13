using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Spawn/Shotgun")]
public class ShotgunPattern : SpawnPattern
{
    [Header("Shotgun settings")]
    public int pelletCount = 5;
    public float spreadAngle = 25f; // fokban
    public float damageMultiplierPerPellet = 0.7f;

    public override IEnumerable<Shot> BuildShots(WeaponContext ctx, targetInfo target)
    {
        int totalPellets = Mathf.Max(1, pelletCount + ctx.weapon.definition.ProjectileCount - 1);
        float dynamicSpread = spreadAngle + ctx.weapon.definition.ProjectileCount * 10f;
        float half = dynamicSpread * 0.5f;

        Vector3 baseDir = target.hasTarget
            ? target.direction
            : ctx.firePoint.right;

        float baseDamage =
            ctx.weapon.definition.Damage *
            damageMultiplierPerPellet;

        float speed = 10;
        float range = ctx.weapon.definition.baseRange;

        if (pelletCount <= 1)
        {
            yield return MakeShot(ctx, baseDir, baseDamage, speed, range);
            yield break;
        }

        for (int i = 0; i < totalPellets; i++)
        {
            float t = totalPellets == 1 ? 0f : i / (totalPellets - 1f);
            float angle = Mathf.Lerp(-half, half, t);

            Vector3 dir =
                Quaternion.Euler(0f, 0f, angle) * baseDir;

            yield return MakeShot(ctx, dir, baseDamage, speed, range);
        }
    }

    private Shot MakeShot(
        WeaponContext ctx,
        Vector3 dir,
        float damage,
        float speed,
        float range)
    {
        return new Shot
        {
            position = ctx.firePoint.position,
            direction = dir.normalized,
            damage = damage,
            speed = speed,
            range = range
        };
    }
}
