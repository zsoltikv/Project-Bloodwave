using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Weapons/Orbiting Weapon Factory")]
public class OrbitingWeaponFactory : ScriptableObject
{
    public GameObject orbitingPrefab;

    public List<GameObject> Spawn(WeaponContext context)
    {
        var orbitList = new List<GameObject>();

        int count = context.weapon.definition.ProjectileCount;

        if (count > 6) count = 6;

        float radius = context.weapon.definition.baseRange;

        for (int i = 0; i < count; i++)
        {
            float startAngle = (360f / count) * i;

            var go = Instantiate(orbitingPrefab);
            go.GetComponent<ProjectileEffects>().OnHitModifiers = context.weapon.definition.modifiersOnHit;
            go.GetComponent<ProjectileEffects>().OnKillModifiers = context.weapon.definition.modifiersOnKill;
            var orbit = go.GetComponent<OrbitingWeapon>();

            orbit.Init(
                context.owner.transform,
                radius: 1.5f + context.weapon.level * 0.3f,
                angularSpeed: 180f + context.weapon.level * 30f,
                damage: context.weapon.definition.Damage
            );

            orbit.SetStartAngle(startAngle);
            orbitList.Add(go);
        }

        return orbitList;

    }
}
