using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SaveScript : MonoBehaviour
{
    public GameObject NameInputField;

    private string playerName;
    private float time;

    RunTimer runTimer;

    [Header("DataSaved UI")]
    public TMP_Text dataSavedText;
    public float fadeInTime = 0.2f;
    public float holdTime = 1.6f;
    public float fadeOutTime = 0.2f;
    public bool useUnscaledTime = true;

    private CanvasGroup dataSavedGroup;
    private Coroutine dataSavedRoutine;

    public List<SaveData> saveDataList = new List<SaveData>();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

    private void Start()
    {
        LoadLeaderboard();
        InitDataSavedUI();
    }

    private void InitDataSavedUI()
    {
        if (dataSavedText == null) return;

        dataSavedGroup = dataSavedText.GetComponent<CanvasGroup>();
        if (dataSavedGroup == null)
            dataSavedGroup = dataSavedText.gameObject.AddComponent<CanvasGroup>();

        dataSavedGroup.alpha = 0f;
        dataSavedText.gameObject.SetActive(false);
    }

    public void SaveGame()
    {
        if (GameManagerScript.instance != null && GameManagerScript.instance.saveUsedThisRun)
        {
            return;
        }

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

        int level = GameManagerScript.instance.level;

        time = RunTimer.instance.timeElapsed;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        SaveData newSave = new SaveData
        {
            playerName = playerName,
            level = level,
            minutes = minutes,
            seconds = seconds
        };

        saveDataList.Add(newSave);

        if (saveDataList.Count == 1)
        {
            AchievementManager.Instance.UnlockAchievement("first_save");
        }

        if (saveDataList.Count == 10)
        {
            AchievementManager.Instance.UnlockAchievement("leaderboard_master");
        }

        if (saveDataList.Count > 10)
        {
            saveDataList.RemoveRange(10, saveDataList.Count - 10);
        }

        bool savedOk = SaveToFile();

        if (savedOk)
        {
            ShowDataSaved();
        }

        if (GameManagerScript.instance != null)
        {
            GameManagerScript.instance.saveUsedThisRun = true;
        }
    }

    private void ShowDataSaved()
    {
        if (dataSavedText == null) return;
        if (dataSavedGroup == null) InitDataSavedUI();

        if (dataSavedRoutine != null)
            StopCoroutine(dataSavedRoutine);

        dataSavedRoutine = StartCoroutine(DataSavedCoroutine());
    }

    private IEnumerator DataSavedCoroutine()
    {
        dataSavedText.gameObject.SetActive(true);

        yield return FadeCanvasGroup(dataSavedGroup, 0f, 1f, fadeInTime);

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(holdTime);
        else
            yield return new WaitForSeconds(holdTime);

        yield return FadeCanvasGroup(dataSavedGroup, 1f, 0f, fadeOutTime);

        dataSavedText.gameObject.SetActive(false);
        dataSavedRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        cg.alpha = from;

        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    private bool SaveToFile()
    {
        try
        {
            LeaderboardWrapper wrapper = new LeaderboardWrapper { saves = saveDataList };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch
        {
            return false;
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