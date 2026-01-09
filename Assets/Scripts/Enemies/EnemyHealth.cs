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
        StopAllCoroutines();
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        currentSpeed = baseSpeed * (1 - slowAmount);
        yield return new WaitForSeconds(duration);
        ResetSpeed();
    }
}