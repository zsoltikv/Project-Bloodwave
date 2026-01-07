using UnityEngine;

public class WeaponInstance
{
    public WeaponDefinition definition;
    public int level = 1;
    public float cooldownTimer;

    public WeaponInstance(WeaponDefinition definition)
    {
        this.definition = definition;
    }
}
