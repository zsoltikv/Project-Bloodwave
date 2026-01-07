using UnityEngine;

public class ProjectileEffects : MonoBehaviour
{
    public WeaponContext context;
    public WeaponModifier[] OnHitModifiers;
    public WeaponModifier[] OnKillModifiers;

    public void RaiseHit(HitInfo hit)
    {
        if (OnHitModifiers == null) return;
        foreach (var modifier in OnHitModifiers)
        {
            modifier.OnHit(ref context, hit);
        }
    }

    public void RaiseKill(KillInfo kill)
    {
        if (OnKillModifiers == null) return;
        foreach (var modifier in OnKillModifiers)
        {
            modifier.OnKill(ref context, kill);
        }
    }
}