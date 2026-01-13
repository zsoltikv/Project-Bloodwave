using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public float Health = 100f;

    [Header("Base")]
    public float baseDamageMultiplier = 1f;
    public float baseCooldownMultiplier = 1f; // 1 = normál, 0.8 = gyorsabb
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;

    [Header("Runtime buffs (optional)")]

    public float CooldownMultiplier = 0f;     // -0.2 = 20%-kal gyorsabb
  
    public event Action OnProjectileBonusChanged;

    public int XP { get; private set; } = 0;
    public int Coins { get; private set; } = 0;

    public void AddXP(int amount)
    {
        XP += amount;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        SceneManager.LoadScene("GameOver");
    }
}