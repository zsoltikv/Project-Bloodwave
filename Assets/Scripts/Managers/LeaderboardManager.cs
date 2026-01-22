using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");
    private List<SaveData> saveDataList = new List<SaveData>();

    public GameObject Content;
    public GameObject SavePrefab;

    void Start()
    {
        LoadSaves();
        PopulateLeaderboard();
    }

    public void PopulateLeaderboard()
    {
        var header = Instantiate(SavePrefab, Content.transform);
        header.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Player";
        header.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Score";
        header.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Time";

        foreach (var save in saveDataList)
        {
            var SaveEntry = Instantiate(SavePrefab, Content.transform);
            SaveEntry.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = save.playerName;
            SaveEntry.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = save.highScore.ToString();
            SaveEntry.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = save.time.ToString();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform as RectTransform);
    }


    public void LoadSaves()
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

}
