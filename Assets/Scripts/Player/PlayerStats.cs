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
    public float baseCooldownMultiplier = 1f;
    public float baseRangeMultiplier = 1f;
    public float baseProjectileSpeed = 12f;
    public int baseProjectileBonus = 0;
    public int score = 0;
    public float baseCritChance = 0;

    private Animator animator;
    private Camera mainCamera;

    [Header("Runtime buffs (optional)")]

    public float CooldownMultiplier = 0f;     
  
    public event Action OnProjectileBonusChanged;

    [Header("Collected resources")]
    [SerializeField] public int XP = 0;
    [SerializeField] public int Coins = 0;

    [Header("Particles")]
    [SerializeField] private ParticleSystem bloodPrefab;

    private void Start()
    {
        mainCamera = this.GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
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
        RunTimer.instance.StopTimer();
        animator.SetBool("isDead", true);

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.isAlive = false;

        StartCoroutine(WaitForDeathAnimation());
    }

    private System.Collections.IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(0.4f);
        
        if (bloodPrefab != null)
        {
            Vector3 spawnPos = transform.position;

            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spawnPos = sr.bounds.center;

            ParticleSystem blood = Instantiate(bloodPrefab, spawnPos, Quaternion.identity);

            var bloodRenderer = blood.GetComponent<ParticleSystemRenderer>();
            if (bloodRenderer != null && sr != null)
                bloodRenderer.sortingOrder = sr.sortingOrder;
        }
        
        if (mainCamera != null)
        {
            float startSize = mainCamera.orthographicSize;
            float targetSize = 4f;
            float duration = 2.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }
            
            mainCamera.orthographicSize = targetSize;
        }
        
        yield return new WaitForSeconds(1f);
        
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