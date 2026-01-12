using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3;
    private float currentHealth;
    public float baseSpeed = 2f;
    public float currentSpeed;
    public int xpReward = 1;
    public int coinReward = 1;

    [Header("Status Effects")]
    public bool isSlowed = false;
    public bool isBleeding = false;

    public bool IsDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentSpeed = baseSpeed;
    }

    public float TakeDamage(float dmg)
    {
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
            Debug.Log("Slow effect.");
            yield return new WaitForSeconds(duration);
            isSlowed = false;
            ResetSpeed();
        }
    }

    public void ApplyBleed(float bleedDamage, float bleedDuration)
    {
        StartCoroutine(BleedCoroutine(bleedDamage, bleedDuration));
    }

    private IEnumerator BleedCoroutine(float bleedDamage, float bleedDuration)
    {
        if (isBleeding)
            yield break;
        isBleeding = true;
        float timeElapsed = 0f;
        while (timeElapsed < bleedDuration)
        {
            TakeDamage(bleedDamage * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            Debug.Log($"Bleed damage applied: {bleedDamage}");
            yield return null;
        }
        isBleeding = false;
    }
}