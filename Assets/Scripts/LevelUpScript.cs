using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelUpScript : MonoBehaviour
{

    [Header("Options")]
    public GameObject FirstOption;
    public GameObject SecondOption;

    [Header("Upgrade Values")]
    [SerializeField] private float damageIncrement = 5f;
    [SerializeField] private int projectileIncrement = 1;
    [SerializeField] private float cooldownReduction = 0.1f;
    [SerializeField] private float rangeIncrease = 0.15f;
    [SerializeField] private float orbitalSpeedIncrease = 0.2f;

    public GameObject DismissButton;

    private WeaponController weaponController;

    private void Start()
    {
        weaponController = FindObjectOfType<WeaponController>();
    }

    void OnEnable()
    {
        GenerateUpgradeOptions();
    }

    private void GenerateUpgradeOptions()
    {
        if (weaponController == null)
        {
            weaponController = FindObjectOfType<WeaponController>();
            if (weaponController == null) return;
        }

        List<WeaponInstance> availableWeapons = weaponController.GetWeapons();
        
        if (availableWeapons.Count == 0) return;

        
        WeaponUpgrade upgrade1 = GenerateRandomUpgrade(availableWeapons);
        WeaponUpgrade upgrade2 = GenerateRandomUpgrade(availableWeapons);

        
        int attempts = 0;
        while (upgrade2.targetWeapon == upgrade1.targetWeapon && 
               upgrade2.upgradeType == upgrade1.upgradeType && 
               attempts < 10)
        {
            upgrade2 = GenerateRandomUpgrade(availableWeapons);
            attempts++;
        }

        if (FirstOption != null)
        {
            var optionUI = FirstOption.GetComponent<UpgradeOptionUI>();
            if (optionUI == null)
                optionUI = FirstOption.AddComponent<UpgradeOptionUI>();
            optionUI.Setup(upgrade1, this);
        }

        if (SecondOption != null)
        {
            var optionUI = SecondOption.GetComponent<UpgradeOptionUI>();
            if (optionUI == null)
                optionUI = SecondOption.AddComponent<UpgradeOptionUI>();
            optionUI.Setup(upgrade2, this);
        }
    }

    private WeaponUpgrade GenerateRandomUpgrade(List<WeaponInstance> weapons)
    {
        WeaponInstance randomWeapon = weapons[Random.Range(0, weapons.Count)];
        
        bool isOrbitingOnly = randomWeapon.definition.orbitingFactory != null &&
                              (randomWeapon.definition.targeting == null || 
                               randomWeapon.definition.spawnPattern == null || 
                               randomWeapon.definition.projectileFactory == null);
        
        UpgradeType randomType;
        if (isOrbitingOnly)
        {
            UpgradeType[] orbitingUpgrades = { UpgradeType.Damage, UpgradeType.ProjectileCount, UpgradeType.Range, UpgradeType.OrbitalSpeed };
            randomType = orbitingUpgrades[Random.Range(0, orbitingUpgrades.Length)];
        }
        else
        {
            randomType = (UpgradeType)Random.Range(0, System.Enum.GetValues(typeof(UpgradeType)).Length - 1);
        }

        float value = 0f;
        switch (randomType)
        {
            case UpgradeType.Damage:
                value = damageIncrement;
                break;
            case UpgradeType.ProjectileCount:
                value = projectileIncrement;
                break;
            case UpgradeType.Cooldown:
                value = cooldownReduction;
                break;
            case UpgradeType.Range:
                value = rangeIncrease;
                break;
            case UpgradeType.OrbitalSpeed:
                value = orbitalSpeedIncrease;
                break;
        }

        return new WeaponUpgrade
        {
            targetWeapon = randomWeapon,
            upgradeType = randomType,
            value = value,
            weaponController = weaponController
        };
    }

    public void DismissLevelUp()
    {
        GameManagerScript.instance.ResumeGame();
        this.gameObject.SetActive(false);
    }
}
