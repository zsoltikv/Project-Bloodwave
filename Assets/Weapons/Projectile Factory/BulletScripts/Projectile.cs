
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject owner;
    private UnityEngine.Vector3 dir;
    private float speed, damage, range;
    private UnityEngine.Vector3 startPosition;

    public void Init(GameObject owner, UnityEngine.Vector3 direction, float damage, float speed, float range)
    {
        this.owner = owner;
        this.dir = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        this.startPosition = transform.position;
    }

    public void Update()
    {
        transform.position += dir * (speed * Time.deltaTime);
        if (UnityEngine.Vector3.Distance(startPosition, transform.position) > range) Destroy(this.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;

        float dealt = hp.TakeDamage(damage);

        var fx = GetComponent<ProjectileEffects>();
        if (fx != null)
        {
            fx.RaiseHit(new HitInfo { target = other.gameObject, point = transform.position, damage = dealt });

            if (hp.IsDead)
            {
                fx.RaiseKill(new KillInfo { target = other.gameObject, point = transform.position });
                Destroy(other.gameObject);
            }

        }

        Destroy(gameObject);
    }

}
