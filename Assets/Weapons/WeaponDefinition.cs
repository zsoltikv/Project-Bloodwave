using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Definitions")]
public class WeaponDefinition : ScriptableObject
{
    public string weaponName;
    public Sprite icon;

    public float baseCooldown = 1f;
    public int baseProjectileCount = 1;
    public float baseDamage = 10f;
    public float baseRange = 6f;

    public TargetingStrategy targeting;
    public SpawnPattern spawnPattern;
    public ProjectileFactory projectileFactory;

    public WeaponModifier[] modifiersOnHit;
    public WeaponModifier[] modifiersOnKill;
}
