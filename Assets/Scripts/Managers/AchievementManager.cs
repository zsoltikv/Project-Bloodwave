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

        // Tutorial / Intro
        achievements.Add(new Achievement("first_time_player", "First Time Player", "Visit How To Play"));
        achievements.Add(new Achievement("movie_buff", "Movie Buff", "Watch the intro video"));

        // Early gameplay milestones
        achievements.Add(new Achievement("first_steps", "First Steps", "Complete your first run"));
        achievements.Add(new Achievement("first_pause", "Taking a Break", "Pause the game for the first time"));
        achievements.Add(new Achievement("first_restart", "First Restart", "Restart the game"));
        achievements.Add(new Achievement("first_save", "First Save", "Save for the first time"));

        // Combat achievements
        achievements.Add(new Achievement("first_blood", "First Blood", "Kill your first enemy"));
        achievements.Add(new Achievement("slayer_10", "Slayer", "Kill 10 enemies"));
        achievements.Add(new Achievement("slayer_50", "Slayer Master", "Kill 50 enemies"));
        achievements.Add(new Achievement("mass_murderer", "Mass Murderer", "Kill 100 enemies"));

        // Player progression
        achievements.Add(new Achievement("level_5", "Getting Stronger", "Reach level 5"));
        achievements.Add(new Achievement("level_10", "Veteran", "Reach level 10"));

        // Collection / shop achievements
        achievements.Add(new Achievement("rich", "Rich", "Collect 1000 coins"));
        achievements.Add(new Achievement("shopaholic", "Shopaholic", "Buy your first item"));
        achievements.Add(new Achievement("collector", "Collector", "Buy 10 items in total"));
        achievements.Add(new Achievement("arsenal", "Arsenal", "Hold 3 weapons at once"));
        achievements.Add(new Achievement("orbit_master", "Orbit Master", "Activate an orbiting weapon"));

        // Survival
        achievements.Add(new Achievement("survivor_5min", "Survivor", "Survive for 5 minutes"));

        // Fun / miscellaneous
        achievements.Add(new Achievement("music_lover", "Music Lover", "Started your first gameplay track"));
        achievements.Add(new Achievement("completionist", "Completionist", "Unlock all achievements"));
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
            Debug.Log("Achievement Unlocked: " + achievement.title);

            if (achievementId != "completionist" &&
                GetUnlockedCount() == GetTotalCount() - 1)
            {
                UnlockAchievement("completionist");
            }
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