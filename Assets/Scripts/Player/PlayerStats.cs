using System;
using System.Collections;
using UnityEngine;
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
    private Coroutine hpAnimCoroutine;

    [Header("Collected resources")]
    [SerializeField] public int XP = 0;
    [SerializeField] public int Coins = 0;

    public int totalKills = 0;

    private float noHitTime = 0f;
    private bool noHitUnlocked = false;

    private readonly System.Collections.Generic.List<float> recentKillTimes =
        new System.Collections.Generic.List<float>();

    private bool multiKillUnlocked = false;
    private const float multiKillWindow = 2f;
    private const int multiKillRequired = 10;

    private Vector3 lastPos;
    private float afkTime = 0f;
    private bool afkUnlocked = false;

    private float damageTakenThisRun = 0f;
    private bool tank500Unlocked = false;

    private bool multiKill20Unlocked = false;
    private const float multiKill20Window = 3f;
    private const int multiKill20Required = 20;
    private bool endMatchSaveTriggered = false;

    [SerializeField] private float moveEpsilon = 0.01f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameManagerScript.instance.ResumeGame();
        RunTimer.instance.ResetTimer();
        PauseGame.ResetRunPauseFlag();
        RunTimer.instance.StartTimer();

        noHitTime = 0f;
        noHitUnlocked = false;

        lastPos = transform.position;
        afkTime = 0f;
        afkUnlocked = false;

        damageTakenThisRun = 0f;
        tank500Unlocked = false;

        multiKillUnlocked = false;
        multiKill20Unlocked = false;
        recentKillTimes.Clear();

        if (levelupCanvasGroup != null)
        {
            levelupCanvasGroup.alpha = 0f;
            levelupCanvasGroup.interactable = false;
            levelupCanvasGroup.blocksRaycasts = false;
            levelupCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();

        XpBar.GetComponent<Slider>().maxValue = CalculateXPForLevel(Level);
        RefreshXpBar();

        xpSlider = XpBar.GetComponent<Slider>();
        xpSlider.minValue = 0;
        xpSlider.maxValue = CalculateXPForLevel(Level);
        xpSlider.value = XP;
        xpSlider.interactable = false;
    }

    private void Update()
    {
        if (Health <= 0.01f) return;

        if (!noHitUnlocked)
        {
            noHitTime += Time.deltaTime;

            if (noHitTime >= 120f)
            {
                noHitUnlocked = true;
                AchievementManager.Instance.UnlockAchievement("no_hit_2min");
            }
        }

        if (!afkUnlocked)
        {
            Vector3 currentPos = transform.position;
            float moved = (currentPos - lastPos).sqrMagnitude;

            if (moved <= moveEpsilon * moveEpsilon)
            {
                afkTime += Time.deltaTime;

                if (afkTime >= 30f)
                {
                    afkUnlocked = true;
                    AchievementManager.Instance.UnlockAchievement("afk_30s");
                }
            }
            else
            {
                afkTime = 0f;
            }

            lastPos = currentPos;
        }
    }

    private void LateUpdate()
    {
        if (shadowTransform != null && spriteRenderer != null && Health < 0.01f)
        {
            Vector3 shadowScale = shadowTransform.localScale;
            shadowScale.x = Mathf.Abs(transform.localScale.x);
            shadowTransform.localScale = shadowScale;

            Vector3 shadowPos = shadowTransform.position;
            shadowPos.x = spriteRenderer.bounds.center.x;
            shadowPos.y = spriteRenderer.bounds.min.y + shadowYOffset;
            shadowTransform.position = shadowPos;
        }
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

    private int CalculateXPForLevel(int level)
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

        if (!multiKillUnlocked || !multiKill20Unlocked)
        {
            float now = Time.time;
            recentKillTimes.Add(now);

            for (int i = recentKillTimes.Count - 1; i >= 0; i--)
            {
                if (now - recentKillTimes[i] > multiKill20Window)
                    recentKillTimes.RemoveAt(i);
            }

            if (!multiKill20Unlocked && recentKillTimes.Count >= multiKill20Required)
            {
                multiKill20Unlocked = true;
                AchievementManager.Instance.UnlockAchievement("multi_kill_20");
            }

            if (!multiKillUnlocked)
            {
                int count2s = 0;

                for (int i = recentKillTimes.Count - 1; i >= 0; i--)
                {
                    if (now - recentKillTimes[i] <= multiKillWindow)
                        count2s++;
                }

                if (count2s >= multiKillRequired)
                {
                    multiKillUnlocked = true;
                    AchievementManager.Instance.UnlockAchievement("multi_kill_10");
                }
            }
        }
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

    public void TakeDamage(float amount)
    {
        if (amount > 0f)
        {
            noHitTime = 0f;

            if (!tank500Unlocked)
            {
                float effectiveDamage = Mathf.Min(amount, Health);
                damageTakenThisRun += effectiveDamage;
            }
        }

        Health -= amount;
        if (Health < 0) Health = 0;

        if (!tank500Unlocked && Health > 0f && damageTakenThisRun >= 500f)
        {
            tank500Unlocked = true;
            AchievementManager.Instance.UnlockAchievement("tank_500");
        }

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

    private IEnumerator FlashRed()
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
        if (endMatchSaveTriggered) return;
        endMatchSaveTriggered = true;

        _ = MatchSaveManager.TryAutoSaveMatchAsync(this);

        if (!PauseGame.PausedThisRun)
        {
            AchievementManager.Instance.UnlockAchievement("no_pause_run");
        }

        if (RunTimer.instance != null &&
            RunTimer.instance.timeElapsed > 0f &&
            RunTimer.instance.timeElapsed <= 15f)
        {
            AchievementManager.Instance.UnlockAchievement("die_fast_15s");
        }

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

    private IEnumerator WaitForDeathAnimation()
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

        xpAnimCoroutine = StartCoroutine(AnimateXpToTarget(XP));
    }

    public void LevelUp()
    {
        if (Health < 0.01f) return;

        Level++;

        LevelText.GetComponent<TMPro.TextMeshProUGUI>().text = "Level " + Level;

        xpSlider.maxValue = CalculateXPForLevel(Level);
        xpSlider.value = 0f;

        GameManagerScript.instance.PauseGame();

        if (levelupFadeCoroutine != null)
            StopCoroutine(levelupFadeCoroutine);

        levelupFadeCoroutine = StartCoroutine(FadeCanvas(levelupCanvasGroup, 1f));

        if (Level >= 5)
        {
            AchievementManager.Instance.UnlockAchievement("level_5");
        }

        if (Level >= 10)
        {
            AchievementManager.Instance.UnlockAchievement("level_10");
        }

        if (Level >= 15)
        {
            AchievementManager.Instance.UnlockAchievement("level_15");
        }

        if (Level >= 20)
        {
            AchievementManager.Instance.UnlockAchievement("level_20");
        }

        if (Level >= 25)
        {
            AchievementManager.Instance.UnlockAchievement("level_25");
        }

        if (Level >= 50)
        {
            AchievementManager.Instance.UnlockAchievement("level_50");
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
            {
                hpDamageFill.fillAmount = Mathf.Lerp(
                    hpDamageFill.fillAmount,
                    hpFill.fillAmount,
                    Time.deltaTime * (hpLerpSpeed / 2)
                );
            }

            yield return null;
        }

        hpFill.fillAmount = targetFill;
    }
}