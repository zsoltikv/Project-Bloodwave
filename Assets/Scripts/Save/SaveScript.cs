using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class SaveScript : MonoBehaviour
{
    public GameObject NameInputField;

    private string playerName;
    private int highScore;
    private int time;

    public List<SaveData> saveDataList = new List<SaveData>();
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

    void Start()
    {
        LoadLeaderboard();
    }

    public void SaveGame()
    {
        if (GameManagerScript.instance != null && GameManagerScript.instance.saveUsedThisRun)
        {
            return;
        }

        // Név input fieldből ha van
        if (NameInputField != null)
        {
            var inputField = NameInputField.GetComponent<TMP_InputField>();
            if (inputField != null && !string.IsNullOrEmpty(inputField.text))
            {
                playerName = inputField.text;
            }
        }

        // Ha még mindig nincs név, default
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }

        // Score és idő lekérése
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            highScore = playerStats.score; 
        }
        
        time = Mathf.RoundToInt(Time.timeSinceLevelLoad);

        // Új mentés létrehozása
        SaveData newSave = new SaveData
        {
            playerName = playerName,
            highScore = highScore,
            time = time
        };

        saveDataList.Add(newSave);
        
        // Rendezés score szerint csökkenő sorrendben
        saveDataList.Sort((a, b) => b.highScore.CompareTo(a.highScore));

        SaveToFile();
        
        if (GameManagerScript.instance != null)
        {
            GameManagerScript.instance.saveUsedThisRun = true;
        }
    }

    private void SaveToFile()
    {
        try
        {
            LeaderboardWrapper wrapper = new LeaderboardWrapper { saves = saveDataList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (System.Exception e)
        {
            // Save failed silently
        }
    }

    public void LoadLeaderboard()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                saveDataList = wrapper.saves ?? new List<SaveData>();
            }
            else
            {
                saveDataList = new List<SaveData>();
            }
        }
        catch (System.Exception e)
        {
            saveDataList = new List<SaveData>();
        }
    }

    public void ClearLeaderboard()
    {
        saveDataList.Clear();
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }
    }

    public List<SaveData> GetTopScores(int count = 10)
    {
        int actualCount = Mathf.Min(count, saveDataList.Count);
        return saveDataList.GetRange(0, actualCount);
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}

[System.Serializable]
public class LeaderboardWrapper
{
    public List<SaveData> saves;
}
