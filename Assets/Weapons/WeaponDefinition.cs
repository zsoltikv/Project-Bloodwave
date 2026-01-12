using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Definitions")]
public class WeaponDefinition : ScriptableObject
{
    public string weaponName;
    public Sprite icon;

    public float Cooldown = 1f;
    public int ProjectileCount = 1;
    public float Damage = 10f;
    public float baseRange = 6f;

    public TargetingStrategy targeting;
    public SpawnPattern spawnPattern;
    public ProjectileFactory projectileFactory;
    public OrbitingWeaponFactory orbitingFactory;

    public WeaponModifier[] modifiersOnHit;
    public WeaponModifier[] modifiersOnKill;
}
