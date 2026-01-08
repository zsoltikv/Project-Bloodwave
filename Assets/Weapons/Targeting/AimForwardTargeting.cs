using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Targeting/Aim Forward")]
public class AimForwardTargeting : TargetingStrategy
{
    public override targetInfo GetTargets(WeaponContext context)
    {
        Vector3 dir = context.firePoint.right;

        var aim = context.owner.GetComponent<AimDirection2D>();
        if (aim != null && aim.Direction.sqrMagnitude > 0.0001f)
            dir = aim.Direction;

        return new targetInfo
        {
            hasTarget = true,              
            position = context.firePoint.position + dir, 
            direction = dir.normalized
        };
    }
}
