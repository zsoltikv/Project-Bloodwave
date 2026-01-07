using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnPattern : ScriptableObject
{
    public abstract IEnumerable<Shot> BuildShots(WeaponContext context, targetInfo target);
}
