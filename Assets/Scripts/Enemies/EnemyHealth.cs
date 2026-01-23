using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3;
    private float currentHealth;
    public float baseSpeed = 2f;
    public float baseDamage = 10f;
    public float currentSpeed;
    public int xpReward = 1;
    public int coinReward = 1;

    [Header("Status Effects")]
    public bool isSlowed = false;
    public bool isBleeding = false;

    public bool IsDead = false;

    [Header("Damage Indicator")]
    public Vector3 damageTextWorldOffset = new Vector3(0f, 0.6f, 0f);

    [Header("Particles")]
    [SerializeField] private ParticleSystem bloodPrefab;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentSpeed = baseSpeed;
    }

    public float TakeDamage(float dmg) => TakeDamage(dmg, true, "none");

    public float TakeDamage(float dmg, string damageType) => TakeDamage(dmg, true, damageType);

    public float TakeDamage(float dmg, bool showText, string damageType)
    {
        if (IsDead) return 0f;

        if (showText && DamageTextSpawner.Instance != null)
        {
            DamageTextSpawner.Instance.Spawn(dmg, transform.position + damageTextWorldOffset, damageType);
        }

        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            Die();
        }

        return dmg;
    }

    private void Die()
    {
        var player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.AddXP(xpReward);
            player.AddCoins(coinReward);
        }

        IsDead = true;
        if (bloodPrefab != null)
        {
            ParticleSystem blood = Instantiate(bloodPrefab, transform.position, Quaternion.identity);
            blood.GetComponent<ParticleSystemRenderer>().sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder;
            Destroy(gameObject);
        }
    }

    private void ResetSpeed()
    {
        currentSpeed = baseSpeed;
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            currentSpeed = baseSpeed * (1 - slowAmount);    
            yield return new WaitForSeconds(duration);
            isSlowed = false;
            ResetSpeed();
        }
    }

    public void ApplyBleed(int tickDamage, float duration, float tickInterval = 1f, bool showText = true)
    {
        StartCoroutine(BleedCoroutine(tickDamage, duration, tickInterval, showText));
    }

    private IEnumerator BleedCoroutine(int tickDamage, float duration, float tickInterval, bool showText)
    {
        if (isBleeding) yield break;
        isBleeding = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            TakeDamage(tickDamage, showText, "bleed");

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        isBleeding = false;
    }
}