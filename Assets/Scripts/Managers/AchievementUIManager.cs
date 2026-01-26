using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform achievementContainer;
    public GameObject achievementItemPrefab;
    public TextMeshProUGUI progressText;
    public Button backButton;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }

        DisplayAchievements();
    }

    void DisplayAchievements()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementManager instance not found!");
            return;
        }

        foreach (Transform child in achievementContainer)
        {
            Destroy(child.gameObject);
        }

        List<Achievement> achievements = AchievementManager.Instance.GetAllAchievements();

        foreach (Achievement achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, achievementContainer);

            Transform titleTransform = item.transform.Find("Title");
            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = achievement.title;
                    titleText.color = achievement.isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                }
            }

            Transform descTransform = item.transform.Find("Description");
            if (descTransform != null)
            {
                TextMeshProUGUI descText = descTransform.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                {
                    descText.text = achievement.description;
                    descText.color = achievement.isUnlocked ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.4f, 0.4f, 0.4f);
                }
            }

            Transform statusTransform = item.transform.Find("Status");
            if (statusTransform != null)
            {
                TextMeshProUGUI statusText = statusTransform.GetComponent<TextMeshProUGUI>();
                if (statusText != null)
                {
                    statusText.text = achievement.isUnlocked ? "UNLOCKED" : "LOCKED";
                    statusText.color = achievement.isUnlocked ? new Color32(0, 255, 0, 255) : new Color32(255, 0, 0, 255);
                }
            }

            Image background = item.GetComponent<Image>();
            if (background != null)
            {
                background.color = achievement.isUnlocked ? new Color(0.09f, 0.18f, 0.09f, 0.8f) : new Color(0.027f, 0.027f, 0.027f, 0.8f);
            }


        }

        if (progressText != null)
        {
            int unlocked = AchievementManager.Instance.GetUnlockedCount();
            int total = AchievementManager.Instance.GetTotalCount();
            progressText.text = "Achievements: " + unlocked + " / " + total;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(achievementContainer.transform as RectTransform);
    }

    void GoBack()
    {
        FadeManager.Instance.LoadSceneWithFade("MenuScene");
    }

    public void WipeAchievements()
    {
        PlayerPrefs.DeleteKey("AchievementData");
        AchievementManager.Instance.LoadAchievements();
        DisplayAchievements();
    }
}