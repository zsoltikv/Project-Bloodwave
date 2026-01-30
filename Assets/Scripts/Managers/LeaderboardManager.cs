using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject Content;
    public GameObject SavePrefab;

    [Header("Visuals")]
    public Color evenRowColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color oddRowColor = new Color(0.10f, 0.10f, 0.10f, 0.9f);
    public Color firstPlaceColor = new Color(1f, 0.84f, 0.2f);
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f);
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);

    [Header("Animation")]
    public float popInDuration = 0.25f;
    public float popInDelayStep = 0.04f;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");
    private List<SaveData> saveDataList = new List<SaveData>();

    void Start()
    {
        LoadSaves();
        PopulateLeaderboard();
    }

    public void LoadSaves()
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

    public void PopulateLeaderboard()
    {
        if (Content == null || SavePrefab == null)
        {
            Debug.LogError("LeaderboardManager: Content or SavePrefab not assigned!");
            return;
        }

        foreach (Transform child in Content.transform)
            Destroy(child.gameObject);

        var header = Instantiate(SavePrefab, Content.transform);
        SetRowText(header, "Player", "Level", "Time");
        SetRowColor(header, new Color(0f, 0f, 0f, 0.85f));
        header.transform.localScale = Vector3.one * 1.05f;

        saveDataList.Sort((a, b) => b.level.CompareTo(a.level));

        for (int i = 0; i < saveDataList.Count; i++)
        {
            var save = saveDataList[i];
            var entry = Instantiate(SavePrefab, Content.transform);

            if (i == 0) SetRowColor(entry, firstPlaceColor);
            else if (i == 1) SetRowColor(entry, secondPlaceColor);
            else if (i == 2) SetRowColor(entry, thirdPlaceColor);
            else SetRowColor(entry, (i % 2 == 0) ? evenRowColor : oddRowColor);

            SetRowText(entry, save.playerName, save.level.ToString(), $"{save.minutes:00}:{save.seconds:00}");

            entry.transform.localScale = Vector3.zero;
            StartCoroutine(PopIn(entry.transform, i * popInDelayStep));
        }
    }

    private void SetRowColor(GameObject row, Color color)
    {
        var img = row.GetComponent<Image>();
        if (img != null)
            img.color = color;
    }

    private void SetRowText(GameObject row, string player, string level, string time)
    {
        if (row.transform.childCount < 3) return;

        var playerText = row.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        var levelText = row.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        var timeText = row.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        if (playerText != null) playerText.text = player;
        if (levelText != null) levelText.text = level;
        if (timeText != null) timeText.text = time;
    }

    private IEnumerator PopIn(Transform target, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / popInDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, eased);
            yield return null;
        }

        target.localScale = Vector3.one;
    }
}