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

    [Header("Particles")]
    [SerializeField] private ParticleSystem bloodPrefab;

    [Header("Shadow")]
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private float shadowYOffset = -0.5f;

    [Header("Runtime buffs (optional)")]

    public float CooldownMultiplier = 0f;     
  
    public event Action OnProjectileBonusChanged;

    private SpriteRenderer spriteRenderer;

    [Header("HP Bar")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image hpDamageFill;
    [SerializeField] private GameObject hpShake;
    public float hpLerpSpeed = 5f;

    [Header("HP Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    [Header("XP Bar")]
    public Slider xpSlider;
    public float xpLerpSpeed = 6f;

    [Header("Level Up Fade")]
    [SerializeField] private CanvasGroup levelupCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine levelupFadeCoroutine;

    private Coroutine xpAnimCoroutine;

    [Header("Collected resources")]
    [SerializeField] public int XP = 0;
    [SerializeField] public int Coins = 0;

    public int totalKills = 0;

    private void Start()
    {
        mainCamera = this.GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        XpBar.GetComponent<Slider>().maxValue = CalculateXPForLevel(Level);
        RefreshXpBar();

        xpSlider = XpBar.GetComponent<Slider>();

        xpSlider.minValue = 0;
        xpSlider.maxValue = CalculateXPForLevel(Level);
        xpSlider.value = XP;

    }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameManagerScript.instance.ResumeGame();
        RunTimer.instance.ResetTimer();
        RunTimer.instance.StartTimer();
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas, float targetAlpha)
    {
        float startAlpha = canvas.alpha;
        float t = 0f;

        canvas.gameObject.SetActive(true);

        while (!Mathf.Approximately(canvas.alpha, targetAlpha))
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvas.alpha = targetAlpha;

        bool visible = targetAlpha > 0.9f;
        canvas.interactable = visible;
        canvas.blocksRaycasts = visible;

        if (!visible)
            canvas.gameObject.SetActive(false);
    }
            
    private void LateUpdate()
    {
        // Szinkronizálja az árnyék pozícióját és scale-ét a karakterrel
        if (shadowTransform != null && spriteRenderer != null && Health < 0.01f)
        {
            // Az árnyék X scale-e a karakter X scale-ével arányos
            Vector3 shadowScale = shadowTransform.localScale;
            shadowScale.x = Mathf.Abs(transform.localScale.x);
            shadowTransform.localScale = shadowScale;

            // Az árnyék pozícióját a karakter bounds centerének alá tesszük
            Vector3 shadowPos = shadowTransform.position;
            shadowPos.x = spriteRenderer.bounds.center.x;
            shadowPos.y = spriteRenderer.bounds.min.y + shadowYOffset;
            shadowTransform.position = shadowPos;
        }
    }

    private void SpawnBlood()
    {
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
    }

    int CalculateXPForLevel(int level)
    {
        int linearPart = level * 40;
        float exponentialPart = 120f * Mathf.Pow(1.18f, level);

        return Mathf.RoundToInt(linearPart + exponentialPart);
    }
    public void AddKill()
    {
        totalKills++;

        if (totalKills == 1)
            AchievementManager.Instance.UnlockAchievement("first_blood");
        if (totalKills == 10)
            AchievementManager.Instance.UnlockAchievement("slayer_10");
        if (totalKills == 50)
            AchievementManager.Instance.UnlockAchievement("slayer_50");
        if (totalKills == 100)
            AchievementManager.Instance.UnlockAchievement("mass_murderer");
    }

    private IEnumerator AnimateXpToTarget(int targetXP)
    {
        float startValue = xpSlider.value;
        float targetValue = targetXP;

        float t = 0f;

        while (!Mathf.Approximately(xpSlider.value, targetValue))
        {
            t += Time.deltaTime * xpLerpSpeed;
            xpSlider.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        xpSlider.value = targetValue;
    }
    public void AddCoins(int amount)
    {
        Coins += amount;

        if (Coins >= 1000)
        {
            AchievementManager.Instance.UnlockAchievement("rich");
        }
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

        SpawnBlood();

        if (Health <= 0)
            Die();
    }

    private IEnumerator ShakeHpBar()
    {
        RectTransform rt = hpShake.GetComponent<RectTransform>();
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

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
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
        {
            movement.isAlive = false;
            movement.EndDrag();
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        DisableShopAndPause();

        GameManagerScript.instance.GetLevel(Level);
        StartCoroutine(WaitForDeathAnimation());

        AchievementManager.Instance.UnlockAchievement("first_steps");
    }
    
    private void DisableShopAndPause()
    {
        if (ShopManager.instance != null)
        {
            ShopManager.instance.DisableShopUI();
        }

        PauseGame pause = FindObjectOfType<PauseGame>();
        if (pause != null)
        {
            pause.enabled = false;
        }
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
        if (hpFill != null)
            hpFill.fillAmount = Health / MaxHealth;
    }

    public void RefreshXpBar()
    {
        if (xpAnimCoroutine != null)
            StopCoroutine(xpAnimCoroutine);

        xpAnimCoroutine = StartCoroutine(
            AnimateXpToTarget(XP)
        );
    }

    public void LevelUp()
    {
        if (Health < 0.01f) return;

        Level++;

        LevelText.GetComponent<TMPro.TextMeshProUGUI>().text =
            "Level " + Level;

        xpSlider.maxValue = CalculateXPForLevel(Level);
        xpSlider.value = 0f;

        GameManagerScript.instance.PauseGame();
        if (levelupFadeCoroutine != null)
            StopCoroutine(levelupFadeCoroutine);

        levelupFadeCoroutine = StartCoroutine(
            FadeCanvas(levelupCanvasGroup, 1f)
        );

        if (Level >= 5)
        {
            AchievementManager.Instance.UnlockAchievement("level_5");
        }

        if (Level >= 10)
        {
            AchievementManager.Instance.UnlockAchievement("level_10");
        }

        Heal(MaxHealth * 0.25f);
    }

    public void AddXP(int amount)
    {
        XP += amount;

        while (XP >= xpSlider.maxValue)
        {
            XP -= Mathf.RoundToInt(xpSlider.maxValue);
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