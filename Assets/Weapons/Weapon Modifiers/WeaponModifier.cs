using UnityEngine;


public abstract class WeaponModifier : ScriptableObject
{
    public virtual void OnBeforeFire(ref WeaponContext context) { }
    public virtual void OnShotBuilt(ref WeaponContext context, ref Shot shot) { }
    public virtual void OnProjectileSpawned(ref WeaponContext context, GameObject projectile) { }

    public virtual void OnHit(ref WeaponContext context, HitInfo hit) {}
    public virtual void OnKill(ref WeaponContext context, KillInfo kill) {}
}

public struct HitInfo
{
    public GameObject target;
    public Vector3 point;
    public float damage;
}

public struct KillInfo
{
    public GameObject target;
    public Vector3 point;
}
