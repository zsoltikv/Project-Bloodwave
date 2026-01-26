using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class SaveScript : MonoBehaviour
{
    public GameObject NameInputField;

    private string playerName;
    private int highScore;
    private float time;

    RunTimer runTimer;

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
            else
            {
                return;
            }
        }


        // Score és idő lekérése
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            highScore = playerStats.score; 
        }

        time = RunTimer.instance.timeElapsed;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        

        // Új mentés létrehozása
        SaveData newSave = new SaveData
        {
            playerName = playerName,
            highScore = highScore,
            minutes = minutes,
            seconds = seconds
        };

        saveDataList.Add(newSave);
        
        // Rendezés score szerint csökkenő sorrendben
        saveDataList.Sort((a, b) => b.highScore.CompareTo(a.highScore));
        
        // Maximum 10 bejegyzés megtartása
        if (saveDataList.Count > 10)
        {
            saveDataList.RemoveRange(10, saveDataList.Count - 10);
        }

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
        FadeManager.Instance.LoadSceneWithFade("MenuScene");
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

    public List<SaveData> GetLeaderboard()
    {
        return saveDataList;
    }

}

[System.Serializable]
public class LeaderboardWrapper
{
    public List<SaveData> saves;
}
