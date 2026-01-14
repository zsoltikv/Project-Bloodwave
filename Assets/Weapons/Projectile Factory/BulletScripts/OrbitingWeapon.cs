using UnityEngine;

public class OrbitingWeapon : MonoBehaviour
{
    private float radius = 1.5f;
    private float angularSpeed = 180f; // fok / mp
    private float damage;

    private Transform owner;
    private float angle;

    public void Init(Transform owner, float radius, float angularSpeed, float damage)
    {
        this.owner = owner;
        this.radius = radius;
        this.angularSpeed = angularSpeed;
        this.damage = damage;
    }

    public void SetStartAngle(float angle)
    {
        this.angle = angle;
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        angle += angularSpeed * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = owner.position + offset;
        transform.rotation = Quaternion.Euler(0, 0, angle - 45f);
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
    }
}
