using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnPattern : ScriptableObject
{
    [Min(0f)]
    public float shotDelay = 0f;
    public abstract IEnumerable<Shot> BuildShots(WeaponContext context, targetInfo target);
}
