using System;
using UnityEngine;

[Serializable]
public class Achievement
{
    public int apiId;
    public string id;
    public string title;
    public string description;
    public bool isUnlocked;

    public Achievement(int apiId, string id, string title, string description)
    {
        this.apiId = apiId;
        this.id = id;
        this.title = title;
        this.description = description;
        isUnlocked = false;
    }
}

[Serializable]
public class AchievementUnlockResponse
{
    public int id;
    public int userId;
    public int achievmentId;
    public string unlockedAt;
}

public static class JsonArrayHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }

    public static T[] FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<T>();

        string wrappedJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper?.items ?? Array.Empty<T>();
    }
}
