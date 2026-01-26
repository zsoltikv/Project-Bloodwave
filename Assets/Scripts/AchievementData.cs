using System;
using UnityEngine;

[Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool isUnlocked;

    public Achievement(string id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.isUnlocked = false;
    }
}

[Serializable]
public class AchievementSaveData
{
    public string[] unlockedAchievementIds;
}