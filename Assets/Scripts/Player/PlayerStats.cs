using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Ui Elements")]
    public GameObject XpBar;
    public GameObject HpBar;
    public GameObject LevelupPanel;

    [Header("Base")]
    public float Health = 100f;
    public float baseDamageMultiplier = 1f;
    public float baseCooldownMultiplier = 1f; // 1 = normál, 0.8 = gyorsabb
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;

    [Header("Runtime buffs (optional)")]

    public float CooldownMultiplier = 0f;     // -0.2 = 20%-kal gyorsabb
  
    public event Action OnProjectileBonusChanged;

    [Header("Collected resources")]
    [SerializeField] public int XP = 0;
    [SerializeField] public int Coins = 0;

    public void AddXP(int amount)
    {
        XP += amount;
        RefreshXpBar();
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

    public void Die()
    {
        FadeManager.Instance.LoadSceneWithFade("GameOver");
    }

    public void RefreshHpBar()
    {
        HpBar.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().fillAmount = Health / 100f;
    }

    public void RefreshXpBar()
    {
        XpBar.GetComponent<UnityEngine.UI.Slider>().value = XP;

        if (XpBar.GetComponent<UnityEngine.UI.Slider>().value >= XpBar.GetComponent<UnityEngine.UI.Slider>().maxValue)
        {

            LevelUp();
        }
    }

    public void LevelUp() 
    {
        XpBar.GetComponent<UnityEngine.UI.Slider>().value = 0f;
        XP = 0;
        XpBar.GetComponent<UnityEngine.UI.Slider>().maxValue = Mathf.RoundToInt(XpBar.GetComponent<UnityEngine.UI.Slider>().maxValue * 1.5f);
        RefreshXpBar();
        GameManagerScript.instance.PauseGame();
        LevelupPanel.SetActive(true);
    }

}