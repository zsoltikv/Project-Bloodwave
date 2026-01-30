using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject Content;
    public GameObject SavePrefab;

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

        // Töröljük a régi sorokat
        foreach (Transform child in Content.transform)
            Destroy(child.gameObject);

        // Header sor
        var header = Instantiate(SavePrefab, Content.transform);
        if (header.transform.childCount >= 3)
        {
            header.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Player";
            header.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Level";
            header.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Time";
        }
        header.transform.localScale = Vector3.one * 1.05f; // kicsit nagyobb a header

        // Mentett adatok sorai
        for (int i = 0; i < saveDataList.Count; i++)
        {
            var save = saveDataList[i];
            var entry = Instantiate(SavePrefab, Content.transform);

            if (entry.transform.childCount >= 3)
            {
                var playerText = entry.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                var levelText = entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                var timeText = entry.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

                playerText.text = save.playerName;
                levelText.text = save.level.ToString();
                timeText.text = $"{save.minutes:00}:{save.seconds:00}";

                // Ha ez az elsõ sor, színezzük aranyra
                if (i == 0)
                {
                    Color goldColor = new Color(1f, 0.84f, 0f); // RGB arany
                    playerText.color = goldColor;
                    levelText.color = goldColor;
                    timeText.color = goldColor;
                }
            }

            // Pop-in animáció
            entry.transform.localScale = Vector3.zero;
            StartCoroutine(PopIn(entry.transform, i * popInDelayStep));
        }
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