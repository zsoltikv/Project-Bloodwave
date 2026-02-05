using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplayManager : MonoBehaviour
{
    [SerializeField] WeaponController weaponController;
    [SerializeField] List<GameObject> weaponDisplays = new();

    private WeaponInstance currentWeapon;

    void Awake()
    {
        for (int i = 0; i < weaponDisplays.Count; i++)
        {
            int index = i;

            var btn = weaponDisplays[i].GetComponent<Button>();
            if (btn == null) btn = weaponDisplays[i].AddComponent<Button>();

            btn.onClick.RemoveAllListeners();

            btn.onClick.AddListener(() =>
            {
                var weapons = weaponController.GetWeapons();
                if (weapons == null || index >= weapons.Count) return;

                weaponController.OpenSwitchPanel(weapons[index]);
            });
        }
    }


    void FixedUpdate()
    {
        List<WeaponInstance> weapons = weaponController.GetWeapons();

        if (weapons.Count <= 0) return;

        for (int i = 0; i < weapons.Count; i++)
        {
            weaponDisplays[i].GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.name == "Weapon").sprite = weapons[i].definition.icon;
            weaponDisplays[i].GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.name == "Cooldown").fillAmount = Mathf.Clamp01(weapons[i].cooldownTimer / weapons[i].GetCooldown());
        }
    }
}