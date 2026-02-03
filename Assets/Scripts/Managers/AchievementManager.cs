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

        achievements.Add(new Achievement("first_time_player", "First Time", "Visit How To Play"));
        achievements.Add(new Achievement("movie_buff", "Movie Buff", "Watch the intro video"));

        achievements.Add(new Achievement("first_pause", "Taking a Break", "Pause the game"));
        achievements.Add(new Achievement("first_restart", "First Restart", "Restart the game"));
        achievements.Add(new Achievement("first_save", "First Save", "Save for the first time"));
        achievements.Add(new Achievement("first_steps", "First Steps", "Complete your first run"));

        achievements.Add(new Achievement("first_blood", "First Blood", "Kill your first enemy"));
        achievements.Add(new Achievement("slayer_10", "Slayer", "Kill 10 enemies"));
        achievements.Add(new Achievement("slayer_50", "Slayer Master", "Kill 50 enemies"));
        achievements.Add(new Achievement("mass_murderer", "Mass Murderer", "Kill 100 enemies"));

        achievements.Add(new Achievement("multi_kill_10", "Boom!", "Kill 10 enemies within 2 seconds"));
        achievements.Add(new Achievement("multi_kill_20", "Nuke!", "Kill 20 enemies within 3 seconds"));

        achievements.Add(new Achievement("no_hit_2min", "Untouchable", "Survive for 2 minutes without taking damage"));
        achievements.Add(new Achievement("tank_500", "Iron Skin", "Take 500 total damage in a single run and survive"));

        achievements.Add(new Achievement("die_fast_15s", "Oops", "Die within 15 seconds"));
        achievements.Add(new Achievement("no_pause_run", "No Breaks", "Complete a run without pausing"));
        achievements.Add(new Achievement("afk_30s", "Statue", "Don't move for 30 seconds and survive"));

        achievements.Add(new Achievement("survivor_5min", "Survivor", "Survive for 5 minutes"));
        achievements.Add(new Achievement("survivor_10min", "Endurer", "Survive for 10 minutes"));
        achievements.Add(new Achievement("survivor_15min", "Unbreakable", "Survive for 15 minutes"));
        achievements.Add(new Achievement("survivor_30min", "Immortal Run", "Survive for 30 minutes"));

        achievements.Add(new Achievement("level_5", "Getting Stronger", "Reach level 5"));
        achievements.Add(new Achievement("level_10", "Veteran", "Reach level 10"));
        achievements.Add(new Achievement("level_15", "Elite", "Reach level 15"));
        achievements.Add(new Achievement("level_20", "Master", "Reach level 20"));
        achievements.Add(new Achievement("level_25", "Legend", "Reach level 25"));
        achievements.Add(new Achievement("level_50", "Immortal", "Reach level 50"));

        achievements.Add(new Achievement("first_weapon_upgrade", "First Upgrade", "Apply your first weapon upgrade."));
        achievements.Add(new Achievement("upgrade_damage_once", "Hard Hitter", "Apply a Damage upgrade."));
        achievements.Add(new Achievement("upgrade_projectiles_once", "More Bullets", "Apply a Projectile Count upgrade."));
        achievements.Add(new Achievement("upgrade_cooldown_once", "Rapid Fire", "Apply a Cooldown upgrade."));
        achievements.Add(new Achievement("upgrade_range_once", "Long Reach", "Apply a Range upgrade."));
        achievements.Add(new Achievement("upgrade_orbitalspeed_once", "Faster Orbit", "Apply an Orbital Speed upgrade."));

        achievements.Add(new Achievement("weapon_level_5", "Weapon Specialist", "Level up a weapon to level 5."));
        achievements.Add(new Achievement("weapon_level_10", "Weapon Master", "Level up a weapon to level 10."));

        achievements.Add(new Achievement("projectiles_bonus_3", "Bullet Storm", "Get +3 bonus projectiles on a weapon."));
        achievements.Add(new Achievement("cooldown_50", "Machine Gun", "Reduce a weapon's cooldown by 50% or more."));
        achievements.Add(new Achievement("range_150", "Sniper Range", "Increase a weapon's range to 150% or more."));
        achievements.Add(new Achievement("orbitalspeed_200", "Hyper Orbit", "Increase orbital speed to 200% or more."));

        achievements.Add(new Achievement("rich", "Rich", "Collect 1000 coins"));
        achievements.Add(new Achievement("shopaholic", "Shopaholic", "Buy your first item"));
        achievements.Add(new Achievement("shop_clear_10", "Bought Out", "Buy 10 shop items in a single run"));
        achievements.Add(new Achievement("collector", "Collector", "Buy 10 items in total"));
        achievements.Add(new Achievement("big_spender", "Big Spender", "Spend 5000 coins in total"));

        achievements.Add(new Achievement("arsenal", "Arsenal", "Hold 3 weapons at once"));
        achievements.Add(new Achievement("orbit_master", "Orbit Master", "Activate an orbiting weapon"));

        achievements.Add(new Achievement("music_lover", "Music Lover", "Started your first music"));

        achievements.Add(new Achievement("unlock_10_achievements", "Collector I", "Unlock 10 achievements"));
        achievements.Add(new Achievement("unlock_25_achievements", "Collector II", "Unlock 25 achievements"));

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
            CheckMilestoneAchievements();

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
        CheckMilestoneAchievements();
    }

    private void CheckMilestoneAchievements()
    {
        int unlocked = GetUnlockedCount();

        if (unlocked >= 10)
            UnlockAchievement("unlock_10_achievements");

        if (unlocked >= 25)
            UnlockAchievement("unlock_25_achievements");
    }

}