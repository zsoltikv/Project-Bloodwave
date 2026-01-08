using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Projectile Factories/Sword Slash Factory")]
public class SwordSlashFactory : ProjectileFactory
{
    public GameObject slashPrefab;
    public Vector2 hitboxOffset = new Vector2(1.1f, 0f); // előre
    public float rotationOffsetDeg = 0f;

    public override GameObject SpawnAndReturn(WeaponContext context, Shot shot)
    {
        Vector3 dir = shot.direction.normalized;

        // Pozíció: firePoint + irány * offset
        Vector3 pos = context.firePoint.position + (Vector3)( (Vector2)dir * hitboxOffset.x );

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffsetDeg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        var go = Object.Instantiate(slashPrefab, pos, rot);
        go.transform.SetParent(context.owner.transform, worldPositionStays: true); 

        var slash = go.GetComponent<SwordSlash>();
        if (slash != null)
            slash.Init(context.owner, shot.damage);

        return go;
    }
}
