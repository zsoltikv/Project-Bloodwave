
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Targeting/Nearest Enemy")]
public class NearestEnemyTargeting : TargetingStrategy
{
    public float maxDistance = 10f;
    public LayerMask enemyMask;

    public override targetInfo GetTargets(WeaponContext context)
    {
        Vector2 pos = context.firePoint.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, maxDistance, enemyMask);

        if (hits == null || hits.Length == 0) return new targetInfo { hasTarget = false, direction = Vector3.right };

        Collider2D best = hits[0];
        float bestDist = ((Vector2)best.transform.position - pos).sqrMagnitude;

        for (int i = 1; i < hits.Length; i++)
        {
            float d = ((Vector2)hits[i].transform.position - pos).sqrMagnitude;
            if (d < bestDist)
            {
                best = hits[i];
                bestDist = d;
            }
        }

        Vector3 dir = ((Vector2)best.transform.position - pos).normalized;
        return new targetInfo { hasTarget = true, position = best.transform.position, direction = dir };
    }
}
