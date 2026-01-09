using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{
    public float lifetime = 0.12f;

    private GameObject owner;
    private float damage;
    private HashSet<EnemyHealth> hit = new();

    public void Init(GameObject owner, float damage)
    {
        this.owner = owner;
        this.damage = damage;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;
        if (!other.TryGetComponent<EnemyHealth>(out var hp)) return;

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
        Debug.Log("Sword hit " + other.gameObject.name + " for " + dealt + " damage.");
    }
}
