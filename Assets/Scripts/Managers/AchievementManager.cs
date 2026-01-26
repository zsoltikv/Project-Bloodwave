using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    private List<Achievement> achievements = new List<Achievement>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAchievements()
    {
        achievements.Clear();

        achievements.Add(new Achievement("first_steps", "First Steps", "Complete your first run"));
        achievements.Add(new Achievement("movie_buff", "Movie Buff", "Watch the intro video"));
    }

    public void UnlockAchievement(string achievementId)
    {
        Achievement achievement = null;
        foreach (Achievement a in achievements)
        {
            if (a.id == achievementId)
            {
                achievement = a;
                break;
            }
        }

        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true;
            SaveAchievements();
            Debug.Log($"Achievement Unlocked: {achievement.title}");
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        foreach (Achievement a in achievements)
        {
            if (a.id == achievementId)
            {
                return a.isUnlocked;
            }
        }
        return false;
    }

    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements);
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (Achievement a in achievements)
        {
            if (a.isUnlocked)
                count++;
        }
        return count;
    }

    public int GetTotalCount()
    {
        return achievements.Count;
    }

    void SaveAchievements()
    {
        List<string> unlockedIds = new List<string>();
        foreach (Achievement a in achievements)
        {
            if (a.isUnlocked)
            {
                unlockedIds.Add(a.id);
            }
        }

        AchievementSaveData saveData = new AchievementSaveData();
        saveData.unlockedAchievementIds = unlockedIds.ToArray();

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("AchievementData", json);
        PlayerPrefs.Save();
    }

    public void LoadAchievements()
    {
        foreach (Achievement a in achievements)
        {
            a.isUnlocked = false;
        }

        if (PlayerPrefs.HasKey("AchievementData"))
        {
            string json = PlayerPrefs.GetString("AchievementData");
            AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(json);

            if (saveData != null && saveData.unlockedAchievementIds != null)
            {
                foreach (string id in saveData.unlockedAchievementIds)
                {
                    foreach (Achievement a in achievements)
                    {
                        if (a.id == id)
                        {
                            a.isUnlocked = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}