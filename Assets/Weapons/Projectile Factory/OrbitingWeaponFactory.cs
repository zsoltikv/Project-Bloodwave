using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Orbiting Weapon Factory")]
public class OrbitingWeaponFactory : ScriptableObject
{
    public GameObject orbitingPrefab;

    public List<GameObject> Spawn(WeaponContext context)
    {
        var orbitList = new List<GameObject>();

        int count = context.weapon.GetProjectileCount();

        if (count > 6) count = 6;

        float radius = context.weapon.GetRange();

    
        float baseRadius = context.weapon.definition.baseRange;
        float baseAngularSpeed = 90f;
        float angularSpeed = baseAngularSpeed * (baseRadius / radius) * context.weapon.GetOrbitalSpeed();

        for (int i = 0; i < count; i++)
        {
            float startAngle = (360f / count) * i;

            var go = Instantiate(orbitingPrefab);
            go.GetComponent<ProjectileEffects>().OnHitModifiers = context.weapon.definition.modifiersOnHit;
            go.GetComponent<ProjectileEffects>().OnKillModifiers = context.weapon.definition.modifiersOnKill;
            var orbit = go.GetComponent<OrbitingWeapon>();

            orbit.Init(
                context.owner.transform,
                radius: radius,
                angularSpeed: angularSpeed,
                damage: context.weapon.GetDamage()
            );

            orbit.SetStartAngle(startAngle);
            orbitList.Add(go);
        }

        return orbitList;

    }
}
