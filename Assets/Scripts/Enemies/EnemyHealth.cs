using System.Data.SqlTypes;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3;
    private float currentHealth;

    public int xpReward = 1;
    public int coinReward = 1;

    public bool IsDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
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
}