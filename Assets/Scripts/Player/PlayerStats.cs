using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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

    private SpriteRenderer spriteRenderer;

    [Header("HP Bar")]
    public Image hpFill;     
    public Image hpDamageFill;  
    public float hpLerpSpeed = 5f;

    [Header("HP Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;    

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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (HpBar != null)
        {
            hpFill = HpBar.transform.GetChild(1).GetComponent<Image>();
            hpDamageFill = HpBar.transform.GetChild(2).GetComponent<Image>(); // pl. a háttér sáv
        }
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

    private Coroutine hpAnimCoroutine;

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;

        if (hpAnimCoroutine != null) StopCoroutine(hpAnimCoroutine);
        hpAnimCoroutine = StartCoroutine(AnimateHpChange());

        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (hpFill != null)
            StartCoroutine(ShakeHpBar());

        if (Health <= 0)
            Die();
    }

    private IEnumerator ShakeHpBar()
    {
        RectTransform rt = hpFill.GetComponent<RectTransform>();
        Vector3 originalPos = rt.anchoredPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float x = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);

            rt.anchoredPosition = originalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }

    private System.Collections.IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    public void Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;

        if (hpAnimCoroutine != null) StopCoroutine(hpAnimCoroutine);
        hpAnimCoroutine = StartCoroutine(AnimateHpChange());
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

    private IEnumerator AnimateHpChange()
    {
        float startFill = hpFill.fillAmount;
        float targetFill = Health / MaxHealth;

        if (targetFill < startFill)
            hpDamageFill.fillAmount = startFill;

        float t = 0f;
        while (!Mathf.Approximately(hpFill.fillAmount, targetFill))
        {
            t += Time.deltaTime * hpLerpSpeed;
            hpFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);

            if (hpDamageFill.fillAmount > hpFill.fillAmount)
                hpDamageFill.fillAmount = Mathf.Lerp(hpDamageFill.fillAmount, hpFill.fillAmount, Time.deltaTime * (hpLerpSpeed / 2));

            yield return null;
        }

        hpFill.fillAmount = targetFill;
    }

}