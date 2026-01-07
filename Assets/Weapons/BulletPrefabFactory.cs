using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Projectile Factories/Bullet Prefab Factory")]
public class BulletPrefabFactory : ProjectileFactory
{
    public GameObject bulletPrefab;

    public override GameObject Spawn(WeaponContext context, Shot shot)
    {
        GameObject bullet = Object.Instantiate(bulletPrefab, shot.position, Quaternion.identity);
        Projectile bulletComponent = bullet.GetComponent<Projectile>();
        if (bulletComponent != null)
        {
            bulletComponent.Init(context.owner, shot.direction, shot.damage, shot.speed, shot.range);
        }
        var fx = bullet.GetComponent<ProjectileEffects>();
        if (fx != null)
        {
            fx.context = context;
            fx.OnHitModifiers = context.weapon.definition.modifiersOnHit;
            fx.OnKillModifiers = context.weapon.definition.modifiersOnKill;
        }
        return bullet;
    }
}
