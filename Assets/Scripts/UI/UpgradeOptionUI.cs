using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeOptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    
    private WeaponUpgrade upgrade;
    private LevelUpScript levelUpScript;

    public void Setup(WeaponUpgrade upgrade, LevelUpScript levelUpScript)
    {
        this.upgrade = upgrade;
        this.levelUpScript = levelUpScript;
        
        if (descriptionText != null)
            descriptionText.text = upgrade.GetDescription();
        
        if (iconImage != null && upgrade.targetWeapon.definition.icon != null)
            iconImage.sprite = upgrade.targetWeapon.definition.icon;

        if (titleText != null)
            titleText.text = upgrade.targetWeapon.definition.weaponName;
        
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        upgrade.Apply();
        levelUpScript.DismissLevelUp();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }
}
