using UnityEngine;

public struct WeaponContext
{
    public GameObject owner;
    public Transform firePoint;
    public Stats stats;
    public WeaponInstance weapon;

}

public struct targetInfo
{
    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 direction;
    public bool hasTarget;
}

public struct Shot
{
    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 direction;
    public float damage;
    public float speed;
    public float range;
}
