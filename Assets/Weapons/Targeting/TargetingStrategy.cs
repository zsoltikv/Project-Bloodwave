using UnityEngine;

public abstract class TargetingStrategy : ScriptableObject
{
    public abstract targetInfo GetTargets(WeaponContext context);
}
