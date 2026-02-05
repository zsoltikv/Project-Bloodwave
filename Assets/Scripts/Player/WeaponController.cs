using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform firePoint;

    [SerializeField] private WeaponDefinition startingWeapon;

    [Header("Runtime Weapons (Debug View)")]
    [SerializeField] private List<WeaponInstance> weapons = new List<WeaponInstance>();
    [SerializeField] private List<WeaponInstance> weaponInventory = new List<WeaponInstance>();

    public List<WeaponInstance> GetWeapons() => weapons;
    public List<WeaponInstance> GetAllWeapons() => weapons.Concat(weaponInventory).ToList();

    private List<GameObject> orbitingObjects = new List<GameObject>();

    [Header("Switch Panel Animation")]
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.9f, 0.9f, 0.9f);

    private CanvasGroup panelCanvasGroup;
    private CanvasGroup overlayCanvasGroup;

    private Coroutine panelAnimRoutine;
    private Coroutine overlayAnimRoutine;

    private Vector3 panelOriginalScale;
    private Vector3 overlayOriginalScale;


    [SerializeField] private GameObject closeOverlay;
    [SerializeField] private GameObject weaponSwitchPanel;
    [SerializeField] private GameObject currentWeaponDisplay;
    [SerializeField] private GameObject weaponSwitchContainer;
    [SerializeField] private GameObject weaponSwitchPrefab;
    private bool IsShootingWeapon(WeaponDefinition def) =>
        def.targeting != null && def.spawnPattern != null && def.projectileFactory != null;

    private void OnEnable()
    {
        stats.OnProjectileBonusChanged += RefreshAllOrbitingWeapons;
    }

    private void OnDisable()
    {
        stats.OnProjectileBonusChanged -= RefreshAllOrbitingWeapons;
    }

    private void Start()
    {
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
    }

    private void Awake()
    {
        if (weaponSwitchPanel != null)
        {
            panelCanvasGroup = weaponSwitchPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = weaponSwitchPanel.AddComponent<CanvasGroup>();
            panelOriginalScale = weaponSwitchPanel.transform.localScale;

            weaponSwitchPanel.SetActive(false);
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        // overlay canvasgroup auto
        if (closeOverlay != null)
        {
            overlayCanvasGroup = closeOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null) overlayCanvasGroup = closeOverlay.AddComponent<CanvasGroup>();
            overlayOriginalScale = closeOverlay.transform.localScale;

            // induláskor rejtve
            closeOverlay.SetActive(false);
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;

            // ha van Button az overlayen, akkor ráakasztjuk a Close-t
            var btn = closeOverlay.GetComponent<Button>();
            if (btn == null) btn = closeOverlay.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(CloseSwitchPanel);

            // overlaynek kell egy Graphic (Image) a raycast-hoz
            var img = closeOverlay.GetComponent<Image>();
            if (img == null) img = closeOverlay.AddComponent<Image>();
            img.raycastTarget = true;
            // átlátszó is lehet
            var c = img.color; c.a = 0f; img.color = c;
        }
    }


    public void AddWeapon(WeaponDefinition definition)
    {
        var instance = new WeaponInstance(definition);

        if(GetWeapons().Count < 3) weapons.Add(instance);
        else weaponInventory.Add(instance);

        if (weapons.Count >= 3)
        {
            AchievementManager.Instance.UnlockAchievement("arsenal");
        }

        RefreshAllOrbitingWeapons();
    }

    public void RefreshAllOrbitingWeapons()
    {
        foreach (var obj in orbitingObjects)
        {
            Destroy(obj);
        }

        orbitingObjects.Clear();

        foreach (var weapon in weapons)
        {
            if (weapon.definition.orbitingFactory == null)
            {
                continue;
            }

            var ctx = new WeaponContext
            {
                owner = gameObject,
                firePoint = firePoint,
                stats = stats,
                weapon = weapon
            };

            ctx.weapon.playerStats = ctx.stats;

            var spawned = weapon.definition.orbitingFactory.Spawn(ctx);
            orbitingObjects.AddRange(spawned);
        }

        if (orbitingObjects.Count > 0)
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.UnlockAchievement("orbit_master");
            }
        }
    }

    private void Update()
    {
        if (GameManagerScript.instance.FreezeGame || stats.Health < 0.01f) return;

        float deltaTime = Time.deltaTime;

        foreach (var weapon in weapons)
        {
            if (!IsShootingWeapon(weapon.definition))
                continue;

            if (!weapon.isFiring)
            {
                weapon.cooldownTimer -= deltaTime;

                if (weapon.cooldownTimer <= 0f)
                {
                    StartCoroutine(FireWeaponRoutine(weapon));
                }
            }
        }
    }

    private float GetCooldown(WeaponInstance weapon)
    {
        return weapon.GetCooldown();
    }

    private IEnumerator FireWeaponRoutine(WeaponInstance _weapon)
    {
        _weapon.isFiring = true;

        var ctx = new WeaponContext
        {
            owner = gameObject,
            firePoint = firePoint,
            stats = stats,
            weapon = _weapon
        };

        ctx.weapon.playerStats = ctx.stats;

        if (_weapon.definition.modifiersOnHit != null)
        {
            foreach (var modifier in _weapon.definition.modifiersOnHit)
            {
                modifier.OnBeforeFire(ref ctx);
            }
        }

        int totalShots;

        if (_weapon.definition.name == "Pistol" ||
            _weapon.definition.name == "BloodScythe" ||
            _weapon.definition.name == "Sword")
        {
            totalShots = ctx.weapon.GetProjectileCount();
        }
        else
        {
            totalShots = 1;
        }

        float delay = _weapon.definition.spawnPattern.shotDelay;

        for (int i = 0; i < totalShots; i++)
        {
            var targetInfo = _weapon.definition.targeting.GetTargets(ctx);

            if (!targetInfo.hasTarget)
            {
                continue;
            }

            var shots = _weapon.definition.spawnPattern.BuildShots(ctx, targetInfo);

            int shotCount = 0;

            foreach (var shot0 in shots)
            {
                shotCount++;
                var shot = shot0;

                if (_weapon.definition.modifiersOnHit != null)
                {
                    foreach (var modifier in _weapon.definition.modifiersOnHit)
                    {
                        modifier.OnShotBuilt(ref ctx, ref shot);
                    }
                }

                var projectile = _weapon.definition.projectileFactory.SpawnAndReturn(ctx, shot);

                if (_weapon.definition.modifiersOnHit != null)
                {
                    foreach (var modifier in _weapon.definition.modifiersOnHit)
                    {
                        modifier.OnProjectileSpawned(ref ctx, projectile);
                    }
                }
            }

            if (i < totalShots - 1)
                yield return new WaitForSeconds(delay);
        }

        _weapon.cooldownTimer = GetCooldown(_weapon);
        _weapon.isFiring = false;
    }

    public void OpenSwitchPanel(WeaponInstance currentWeapon)
    {
        if (currentWeapon == null || weaponInventory.Count == 0) return;

        currentWeaponDisplay
            .GetComponentsInChildren<Image>(true)
            .FirstOrDefault(x => x.name == "Weapon")
            .sprite = currentWeapon.definition.icon;

        RefreshSwitchOptions(currentWeapon);

        GameManagerScript.instance.PauseGame();

        if (overlayAnimRoutine != null) StopCoroutine(overlayAnimRoutine);
        if (panelAnimRoutine != null) StopCoroutine(panelAnimRoutine);

        overlayAnimRoutine = StartCoroutine(OpenAnim(closeOverlay, overlayCanvasGroup, overlayOriginalScale));
        panelAnimRoutine = StartCoroutine(OpenAnim(weaponSwitchPanel, panelCanvasGroup, panelOriginalScale));
    }

    public void CloseSwitchPanel()
    {
        if (overlayAnimRoutine != null) StopCoroutine(overlayAnimRoutine);
        if (panelAnimRoutine != null) StopCoroutine(panelAnimRoutine);

        overlayAnimRoutine = StartCoroutine(CloseAnim(closeOverlay, overlayCanvasGroup, overlayOriginalScale));
        panelAnimRoutine = StartCoroutine(CloseAnim(weaponSwitchPanel, panelCanvasGroup, panelOriginalScale));

        GameManagerScript.instance.ResumeGame();
    }


    private void RefreshSwitchOptions(WeaponInstance currentWeapon)
    {
        for (int i = weaponSwitchContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(weaponSwitchContainer.transform.GetChild(i).gameObject);

        foreach (var weapon in weaponInventory)
        {
            var go = Instantiate(weaponSwitchPrefab, weaponSwitchContainer.transform);

            go.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(x => x.name == "Weapon")
                .sprite = weapon.definition.icon;

            var btn = go.AddComponent<Button>();

            btn.onClick.AddListener(() =>
            {
                int currentIndex = weapons.IndexOf(currentWeapon);
                if (currentIndex >= 0)
                    weapons[currentIndex] = weapon;

                weaponInventory.Remove(weapon);
                weaponInventory.Insert(0, currentWeapon);

                RefreshAllOrbitingWeapons();

                CloseSwitchPanel();
            });
        }
    }

    private IEnumerator OpenAnim(GameObject go, CanvasGroup cg, Vector3 originalScale)
    {
        if (go == null || cg == null) yield break;

        go.SetActive(true);

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        go.transform.localScale = Vector3.Scale(originalScale, hiddenScale);

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = animDuration <= 0f ? 1f : Mathf.Clamp01(t / animDuration);

            cg.alpha = Mathf.Lerp(0f, 1f, lerp);
            go.transform.localScale = Vector3.Lerp(
                Vector3.Scale(originalScale, hiddenScale),
                originalScale,
                EaseOutBack(lerp)
            );

            yield return null;
        }

        cg.alpha = 1f;
        go.transform.localScale = originalScale;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator CloseAnim(GameObject go, CanvasGroup cg, Vector3 originalScale)
    {
        if (go == null || cg == null) yield break;

        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = animDuration <= 0f ? 1f : Mathf.Clamp01(t / animDuration);

            cg.alpha = Mathf.Lerp(1f, 0f, lerp);
            go.transform.localScale = Vector3.Lerp(
                originalScale,
                Vector3.Scale(originalScale, hiddenScale),
                lerp
            );

            yield return null;
        }

        cg.alpha = 0f;
        go.transform.localScale = originalScale;
        go.SetActive(false);
    }

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }


}