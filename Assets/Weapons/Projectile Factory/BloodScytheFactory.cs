using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Projectile Factories/Blood Scythe Factory")]
public class BloodScytheFactory : ProjectileFactory
{
    public GameObject scythePrefab;

    public override GameObject SpawnAndReturn(WeaponContext context, Shot shot)
    {
        Vector3 dir = shot.direction.normalized;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector3 pos = context.owner.transform.position;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        var go = Object.Instantiate(scythePrefab, pos, rot);
        go.transform.SetParent(context.owner.transform, worldPositionStays: true);

        var scythe = go.GetComponent<BloodScythe>();
        if (scythe != null)
        {
            scythe.Init(context.owner, shot.damage, angle, shot.range);
        }

        var fx = go.GetComponent<ProjectileEffects>();
        if (fx != null)
        {
            fx.OnHitModifiers = context.weapon.definition.modifiersOnHit;
            fx.OnKillModifiers = context.weapon.definition.modifiersOnKill;
        }

        return go;
    }
}
