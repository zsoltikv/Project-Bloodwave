using UnityEngine;

public abstract class ProjectileFactory : ScriptableObject
{
    public abstract GameObject Spawn(WeaponContext context, Shot shot);
}
