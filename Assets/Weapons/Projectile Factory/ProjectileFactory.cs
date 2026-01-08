using UnityEngine;

public abstract class ProjectileFactory : ScriptableObject
{
    public abstract GameObject SpawnAndReturn(WeaponContext context, Shot shot);
}
