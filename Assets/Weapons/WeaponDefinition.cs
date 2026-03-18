using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Definitions")]
public class WeaponDefinition : ScriptableObject
{
    [SerializeField] private int weaponId;

    public string weaponName;
    public Sprite icon;

    public float Cooldown = 1f;
    public int ProjectileCount = 1;
    public float Damage = 10f;
    public float baseRange = 6f;
    public float ProjectileSpeed = 1f;

    public TargetingStrategy targeting;
    public SpawnPattern spawnPattern;
    public ProjectileFactory projectileFactory;
    public OrbitingWeaponFactory orbitingFactory;

    public WeaponModifier[] modifiersOnHit;
    public WeaponModifier[] modifiersOnKill;

    public int WeaponId => weaponId > 0 ? weaponId : Mathf.Abs(name.GetHashCode());

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (weaponId <= 0)
        {
            weaponId = System.Math.Abs(System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0));
            if (weaponId == 0) weaponId = 1;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}