using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public static event Action OnAchievementsChanged;

    private const string AchievementSyncEndpoint = "/api/Achievment/me";

    private readonly List<Achievement> achievements = new List<Achievement>();
    private bool _isSyncing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAchievements();
            ResetAchievementState();
            AuthManager.OnSessionChanged += HandleSessionChanged;
            _ = RefreshAchievementsFromApiAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            AuthManager.OnSessionChanged -= HandleSessionChanged;
    }

    private void HandleSessionChanged()
    {
        _ = RefreshAchievementsFromApiAsync();
    }

    private void InitializeAchievements()
    {
        achievements.Clear();

        achievements.Add(new Achievement(1, "first_time_player", "First Time", "Visit How To Play"));
        achievements.Add(new Achievement(2, "movie_buff", "Movie Buff", "Watch the intro video"));

        achievements.Add(new Achievement(3, "first_pause", "Taking a Break", "Pause the game"));
        achievements.Add(new Achievement(4, "first_restart", "First Restart", "Restart the game"));
        achievements.Add(new Achievement(5, "first_save", "First Save", "Save for the first time"));
        achievements.Add(new Achievement(6, "first_steps", "First Steps", "Complete your first run"));

        achievements.Add(new Achievement(7, "first_blood", "First Blood", "Kill your first enemy"));
        achievements.Add(new Achievement(8, "slayer_10", "Slayer", "Kill 10 enemies"));
        achievements.Add(new Achievement(9, "slayer_50", "Slayer Master", "Kill 50 enemies"));
        achievements.Add(new Achievement(10, "mass_murderer", "Mass Murderer", "Kill 100 enemies"));

        achievements.Add(new Achievement(11, "multi_kill_10", "Boom!", "Kill 10 enemies within 2 seconds"));
        achievements.Add(new Achievement(12, "multi_kill_20", "Nuke!", "Kill 20 enemies within 3 seconds"));

        achievements.Add(new Achievement(13, "no_hit_2min", "Untouchable", "Survive for 2 minutes without taking damage"));
        achievements.Add(new Achievement(14, "tank_500", "Iron Skin", "Take 500 total damage in a single run and survive"));

        achievements.Add(new Achievement(15, "die_fast_15s", "Oops", "Die within 15 seconds"));
        achievements.Add(new Achievement(16, "no_pause_run", "No Breaks", "Complete a run without pausing"));
        achievements.Add(new Achievement(17, "afk_30s", "Statue", "Don't move for 30 seconds and survive"));

        achievements.Add(new Achievement(18, "survivor_5min", "Survivor", "Survive for 5 minutes"));
        achievements.Add(new Achievement(19, "survivor_10min", "Endurer", "Survive for 10 minutes"));
        achievements.Add(new Achievement(20, "survivor_15min", "Unbreakable", "Survive for 15 minutes"));
        achievements.Add(new Achievement(21, "survivor_30min", "Immortal Run", "Survive for 30 minutes"));

        achievements.Add(new Achievement(22, "level_5", "Getting Stronger", "Reach level 5"));
        achievements.Add(new Achievement(23, "level_10", "Veteran", "Reach level 10"));
        achievements.Add(new Achievement(24, "level_15", "Elite", "Reach level 15"));
        achievements.Add(new Achievement(25, "level_20", "Master", "Reach level 20"));
        achievements.Add(new Achievement(26, "level_25", "Legend", "Reach level 25"));
        achievements.Add(new Achievement(27, "level_50", "Immortal", "Reach level 50"));

        achievements.Add(new Achievement(28, "first_weapon_upgrade", "First Upgrade", "Apply your first weapon upgrade."));
        achievements.Add(new Achievement(29, "upgrade_damage_once", "Hard Hitter", "Apply a Damage upgrade."));
        achievements.Add(new Achievement(30, "upgrade_projectiles_once", "More Bullets", "Apply a Projectile Count upgrade."));
        achievements.Add(new Achievement(31, "upgrade_cooldown_once", "Rapid Fire", "Apply a Cooldown upgrade."));
        achievements.Add(new Achievement(32, "upgrade_range_once", "Long Reach", "Apply a Range upgrade."));
        achievements.Add(new Achievement(33, "upgrade_orbitalspeed_once", "Faster Orbit", "Apply an Orbital Speed upgrade."));

        achievements.Add(new Achievement(34, "weapon_level_5", "Weapon Specialist", "Level up a weapon to level 5."));
        achievements.Add(new Achievement(35, "weapon_level_10", "Weapon Master", "Level up a weapon to level 10."));

        achievements.Add(new Achievement(36, "projectiles_bonus_3", "Bullet Storm", "Get +3 bonus projectiles on a weapon."));
        achievements.Add(new Achievement(37, "cooldown_50", "Machine Gun", "Reduce a weapon's cooldown by 50% or more."));
        achievements.Add(new Achievement(38, "range_150", "Sniper Range", "Increase a weapon's range to 150% or more."));
        achievements.Add(new Achievement(39, "orbitalspeed_200", "Hyper Orbit", "Increase orbital speed to 200% or more."));

        achievements.Add(new Achievement(40, "rich", "Rich", "Collect 1000 coins"));
        achievements.Add(new Achievement(41, "shopaholic", "Shopaholic", "Buy your first item"));
        achievements.Add(new Achievement(42, "shop_clear_10", "Bought Out", "Buy 10 shop items in a single run"));
        achievements.Add(new Achievement(43, "collector", "Collector", "Buy 10 items in total"));
        achievements.Add(new Achievement(44, "big_spender", "Big Spender", "Spend 5000 coins in total"));

        achievements.Add(new Achievement(45, "arsenal", "Arsenal", "Hold 3 weapons at once"));
        achievements.Add(new Achievement(46, "orbit_master", "Orbit Master", "Activate an orbiting weapon"));

        achievements.Add(new Achievement(47, "music_lover", "Music Lover", "Started your first music"));

        achievements.Add(new Achievement(48, "unlock_10_achievements", "Collector I", "Unlock 10 achievements"));
        achievements.Add(new Achievement(49, "unlock_25_achievements", "Collector II", "Unlock 25 achievements"));
        achievements.Add(new Achievement(50, "completionist", "Completionist", "Unlock all achievements"));
    }

    public void UnlockAchievement(string achievementId)
    {
        Achievement achievement = FindAchievement(achievementId);

        if (achievement == null || achievement.isUnlocked)
            return;

        achievement.isUnlocked = true;
        Debug.Log("Achievement Unlocked: " + achievement.title);
        NotifyAchievementsChanged();

        _ = SendUnlockAchievementToApiAsync(achievement);
        EnsureMetaAchievementsUnlocked();
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        Achievement achievement = FindAchievement(achievementId);
        return achievement != null && achievement.isUnlocked;
    }

    public List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>(achievements);
    }

    public int GetUnlockedCount()
    {
        int count = 0;

        foreach (Achievement achievement in achievements)
        {
            if (achievement.isUnlocked)
                count++;
        }

        return count;
    }

    public int GetTotalCount()
    {
        return achievements.Count;
    }

    public void LoadAchievements()
    {
        _ = RefreshAchievementsFromApiAsync();
    }

    public async Task RefreshAchievementsFromApiAsync()
    {
        if (_isSyncing)
            return;

        _isSyncing = true;

        try
        {
            ResetAchievementState();

            if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn())
            {
                NotifyAchievementsChanged();
                return;
            }

            AchievementUnlockResponse[] unlockedAchievements = await FetchUnlockedAchievementsAsync();

            foreach (AchievementUnlockResponse unlockedAchievement in unlockedAchievements)
            {
                Achievement achievement = FindAchievementByApiId(unlockedAchievement.achievmentId);
                if (achievement != null)
                    achievement.isUnlocked = true;
            }

            NotifyAchievementsChanged();
            EnsureMetaAchievementsUnlocked();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AchievementManager] Failed to sync achievements from API: {ex.Message}");
            NotifyAchievementsChanged();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private Achievement FindAchievement(string achievementId)
    {
        foreach (Achievement achievement in achievements)
        {
            if (achievement.id == achievementId)
                return achievement;
        }

        return null;
    }

    private Achievement FindAchievementByApiId(int apiId)
    {
        foreach (Achievement achievement in achievements)
        {
            if (achievement.apiId == apiId)
                return achievement;
        }

        return null;
    }

    private void ResetAchievementState()
    {
        foreach (Achievement achievement in achievements)
        {
            achievement.isUnlocked = false;
        }
    }

    private void EnsureMetaAchievementsUnlocked()
    {
        int unlocked = GetUnlockedCount();

        if (unlocked >= 10)
            UnlockAchievement("unlock_10_achievements");

        if (unlocked >= 25)
            UnlockAchievement("unlock_25_achievements");

        if (!IsAchievementUnlocked("completionist") && unlocked >= GetTotalCount() - 1)
            UnlockAchievement("completionist");
    }

    private async Task<AchievementUnlockResponse[]> FetchUnlockedAchievementsAsync()
    {
        string response = await AuthManager.Instance.AuthFetchAsync(AchievementSyncEndpoint, "GET");

        if (TryParseUnlockResponseArray(response, out AchievementUnlockResponse[] unlockedAchievements))
            return unlockedAchievements;

        throw new Exception("Achievement sync endpoint did not return a compatible unlock list.");
    }

    private bool TryParseUnlockResponseArray(string response, out AchievementUnlockResponse[] unlockedAchievements)
    {
        unlockedAchievements = Array.Empty<AchievementUnlockResponse>();

        if (string.IsNullOrWhiteSpace(response))
            return false;

        string trimmedResponse = response.Trim();
        if (!trimmedResponse.StartsWith("[") || !trimmedResponse.Contains("achievmentId"))
            return false;

        try
        {
            unlockedAchievements = JsonArrayHelper.FromJson<AchievementUnlockResponse>(trimmedResponse);
            return unlockedAchievements.Length > 0 || trimmedResponse == "[]";
        }
        catch
        {
            return false;
        }
    }

    private async Task SendUnlockAchievementToApiAsync(Achievement achievement)
    {
        if (achievement == null || AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn())
            return;

        try
        {
            await AuthManager.Instance.AuthFetchAsync($"/api/Achievment/{achievement.apiId}/unlock", "POST");
            await RefreshAchievementsFromApiAsync();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AchievementManager] Failed to sync achievement '{achievement.id}' with API: {ex.Message}");
        }
    }

    private void NotifyAchievementsChanged()
    {
        OnAchievementsChanged?.Invoke();
    }
}
