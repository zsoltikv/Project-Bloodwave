using UnityEngine;

[CreateAssetMenu(fileName = "SlowOnHit", menuName = "Weapon Modifiers/Slow On Hit")]
public class SlowOnHit : WeaponModifier
{
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;

    public override void OnHit(ref WeaponContext context, HitInfo hit)
    {
        var targetStats = hit.target.GetComponent<EnemyHealth>();
        if (targetStats != null)
        {
            targetStats.ApplySlow(slowAmount, slowDuration);
        }
    }
}