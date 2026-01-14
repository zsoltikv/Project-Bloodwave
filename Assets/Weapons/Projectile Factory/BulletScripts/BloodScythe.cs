using System.Collections.Generic;
using UnityEngine;

public class BloodScythe : MonoBehaviour
{
    public float lifetime = 0.25f;
    public float sweepAngle = 120f;
    public float sweepSpeed = 720f;

    private GameObject owner;
    private float damage;
    private float range;
    private HashSet<EnemyHealth> hitEnemies = new();
    private float initialAngle;
    private float currentSweepAngle;

    public void Init(GameObject owner, float damage, float startAngle, float range)
    {
        this.owner = owner;
        this.damage = damage;
        this.range = range;
        this.initialAngle = startAngle;
        this.currentSweepAngle = 0f;

        float baseRange = 2.0f;
        float scale = (range / baseRange) * 0.3f;
        transform.localScale = Vector3.one * scale;
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        currentSweepAngle += sweepSpeed * Time.deltaTime;
        float progress = Mathf.Clamp01(currentSweepAngle / sweepAngle);
        
        float sweepOffset = Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, progress);
        float angle = initialAngle + sweepOffset;

        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        float offsetDistance = range * 0.3f; 
        Vector3 positionOffset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * offsetDistance;
        transform.position = owner.transform.position + positionOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;
        if (hitEnemies.Contains(hp)) return; 

        hitEnemies.Add(hp);
        float dealt = hp.TakeDamage(damage);

        var fx = GetComponent<ProjectileEffects>();
        if (fx != null)
        {
            fx.RaiseHit(new HitInfo { target = other.gameObject, point = transform.position, damage = dealt });

            if (hp.IsDead)
            {
                fx.RaiseKill(new KillInfo { target = hp.gameObject, point = transform.position });
                Destroy(hp.gameObject);
            }
        }
    }
}
