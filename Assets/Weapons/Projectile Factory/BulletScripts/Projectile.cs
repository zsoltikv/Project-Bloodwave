using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject owner;
    private Vector3 dir;
    private float speed;
    private float damage;
    private float range;
    private Vector3 startPosition;

    private bool hasHit = false;

    public void Init(GameObject owner, Vector3 direction, float damage, float speed, float range)
    {
        this.owner = owner;
        dir = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.range = range;

        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += dir * (speed * Time.deltaTime);

        if (Vector3.Distance(startPosition, transform.position) > range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || other.gameObject == owner) return;
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;

        hasHit = true;

        float dealt = hp.TakeDamage(damage);

        var fx = GetComponent<ProjectileEffects>();
        if (fx != null)
        {
            fx.RaiseHit(new HitInfo
            {
                target = other.gameObject,
                point = transform.position,
                damage = dealt
            });

            if (hp.IsDead)
            {
                fx.RaiseKill(new KillInfo
                {
                    target = other.gameObject,
                    point = transform.position
                });

                Destroy(other.gameObject);
            }
        }

        Destroy(gameObject);
    }
}