using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform firePoint;
    
    [SerializeField] private WeaponDefinition startingWeapon;

    private readonly List<WeaponInstance> weapons = new List<WeaponInstance>();

    private void Start()
    {
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
    }

    public void AddWeapon(WeaponDefinition definition)
    {
        weapons.Add(new WeaponInstance(definition));
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (var weapon in weapons)
        {
            weapon.cooldownTimer -= deltaTime;
            if (weapon.cooldownTimer <= 0f)
            {
                FireWeapon(weapon);
                weapon.cooldownTimer = GetCooldown(weapon);
            }
        }
    }

    private float GetCooldown(WeaponInstance weapon)
    {
        return weapon.definition.baseCooldown * stats.CooldownMultiplier;
    }

    private void FireWeapon(WeaponInstance _weapon)
    {
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

            var projectile = _weapon.definition.projectileFactory.Spawn(ctx, shot);

            if (_weapon.definition.modifiersOnHit != null)
            {
                foreach (var modifier in _weapon.definition.modifiersOnHit)
                {
                    modifier.OnProjectileSpawned(ref ctx, projectile);
                }
            }
        }
    }

}
