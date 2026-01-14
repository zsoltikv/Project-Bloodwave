using System.Collections.Generic;
using UnityEngine;

public class BloodScythe : MonoBehaviour
{
    public float lifetime = 0.25f;
    public float sweepAngle = 120f; // Wide arc sweep
    public float sweepSpeed = 720f; // Degrees per second

    private GameObject owner;
    private float damage;
    private HashSet<EnemyHealth> hitEnemies = new();
    private float initialAngle;
    private float currentSweepAngle;

    public void Init(GameObject owner, float damage, float startAngle)
    {
        this.owner = owner;
        this.damage = damage;
        this.initialAngle = startAngle;
        this.currentSweepAngle = 0f;
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        // Sweep through arc
        currentSweepAngle += sweepSpeed * Time.deltaTime;
        float progress = Mathf.Clamp01(currentSweepAngle / sweepAngle);
        
        // Swing from -sweepAngle/2 to +sweepAngle/2
        float offset = Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, progress);
        float angle = initialAngle + offset;

        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Keep at owner's position
        transform.position = owner.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;
        if (hitEnemies.Contains(hp)) return; // Each enemy can only be hit once per swing

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
