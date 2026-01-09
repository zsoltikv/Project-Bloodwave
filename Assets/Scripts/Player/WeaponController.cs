using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform firePoint;
    
    [SerializeField] private WeaponDefinition startingWeapon;

    private readonly List<WeaponInstance> weapons = new List<WeaponInstance>();

    private bool IsShootingWeapon(WeaponDefinition def) => def.targeting != null && def.spawnPattern != null && def.projectileFactory != null;
    private List<GameObject> orbitingObjects = new List<GameObject>();

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

    public void AddWeapon(WeaponDefinition definition)
    {
        var instance = new WeaponInstance(definition);
        weapons.Add(instance);

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

            var spawned = weapon.definition.orbitingFactory.Spawn(new WeaponContext
            {
                owner = this.gameObject,
                firePoint = firePoint,
                stats = stats,
                weapon = weapon
            });

            orbitingObjects.AddRange(spawned);
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (var weapon in weapons)
        {
            if (!IsShootingWeapon(weapon.definition))
                continue;

            weapon.cooldownTimer -= deltaTime;
            if (weapon.cooldownTimer <= 0f)
            {
                StartCoroutine(FireWeaponRoutine(weapon));
                weapon.cooldownTimer = GetCooldown(weapon);
            }
        }
    }

    private float GetCooldown(WeaponInstance weapon)
    {
        return weapon.definition.baseCooldown * stats.CooldownMultiplier;
    }

    private IEnumerator FireWeaponRoutine(WeaponInstance _weapon)
    {
        int totalShots;

        var ctx = new WeaponContext
        {
            owner = this.gameObject,
            firePoint = firePoint,
            stats = stats,
            weapon = _weapon
        };

        if (_weapon.definition.modifiersOnHit != null)
        {
            foreach (var modifier in _weapon.definition.modifiersOnHit)
            {
                modifier.OnBeforeFire(ref ctx);
            }
        }

        if (_weapon.definition.name == "Pistol")
        {
            totalShots = 1 + ctx.stats.ProjectileBonus;
        }
        else
        {
            totalShots = 1;
        }
        float delay = _weapon.definition.spawnPattern.shotDelay; 

        for (int i = 0; i < totalShots; i++)
        {
            var targetInfo = _weapon.definition.targeting.GetTargets(ctx);

            var shots = _weapon.definition.spawnPattern.BuildShots(ctx, targetInfo);

            foreach (var shot0 in shots)
            {
                var shot = shot0;

                if (_weapon.definition.modifiersOnHit != null)
                {
                    foreach (var modifier in _weapon.definition.modifiersOnHit)
                    {
                        modifier.OnShotBuilt(ref ctx, ref shot);
                    }
                }

                var projectile =
                    _weapon.definition.projectileFactory.SpawnAndReturn(ctx, shot);

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
    }


}
