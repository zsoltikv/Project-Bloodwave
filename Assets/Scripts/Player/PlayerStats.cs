using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Ui Elements")]
    public GameObject XpBar;
    public GameObject HpBar;
    public GameObject LevelupPanel;
    public GameObject LevelText;

    [Header("Base")]
    public float Health = 100f;
    public float MaxHealth = 100f;
    public int Level = 1;
    public float baseDamageMultiplier = 1f;
    public float baseCooldownMultiplier = 1f; // 1 = normál, 0.8 = gyorsabb
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;
    public int score = 0;
    public float baseCritChance = 0;

    [Header("Runtime buffs (optional)")]

    public float CooldownMultiplier = 0f;     // -0.2 = 20%-kal gyorsabb
  
    public event Action OnProjectileBonusChanged;

    [Header("Collected resources")]
    [SerializeField] public int XP = 0;
    [SerializeField] public int Coins = 0;

    private void Start()
    {
        XpBar.GetComponent<Slider>().maxValue = CalculateXPForLevel(Level);
        RefreshXpBar();
    }

    int CalculateXPForLevel(int level)
    {
        int linearPart = level * 40;
        float exponentialPart = 120f * Mathf.Pow(1.18f, level);

        return Mathf.RoundToInt(linearPart + exponentialPart);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;

        RefreshHpBar();

        if (Health <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        Health += amount;
        if (Health > 100f)
        {
            Health = 100f;
        }
        RefreshHpBar();
    }

    public void Die()
    {
        FadeManager.Instance.LoadSceneWithFade("GameOverScene");
    }

    public void RefreshHpBar()
    {
        HpBar.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = Health / MaxHealth;
    }

    public void RefreshXpBar()
    {
        XpBar.GetComponent<Slider>().value = XP;
    }

    public void LevelUp()
    {
        Level++;

        LevelText.GetComponent<TMPro.TextMeshProUGUI>().text =
            "Level " + Level.ToString();

        XpBar.GetComponent<Slider>().maxValue =
            CalculateXPForLevel(Level);

        GameManagerScript.instance.PauseGame();
        LevelupPanel.SetActive(true);
    }

    public void AddXP(int amount)
    {
        XP += amount;

        while (XP >= XpBar.GetComponent<Slider>().maxValue)
        {
            XP -= Mathf.RoundToInt(XpBar.GetComponent<Slider>().maxValue);
            LevelUp();
        }

        RefreshXpBar();
    }

}