using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    
    [Header("Input")]
    public TMP_InputField nameInputField;
    public Button saveButton;
    
    [Header("Manager")]
    public SaveScript saveScript;

    void Start()
    {
        if (saveScript == null)
        {
            saveScript = FindAnyObjectByType<SaveScript>();
        }
        
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
    }

    public void OnSaveButtonClicked()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            saveScript.SetPlayerName(nameInputField.text);
        }
        
        saveScript.SaveGame();
        RefreshLeaderboard();
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            RefreshLeaderboard();
        }
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    public void RefreshLeaderboard()
    {
        if (leaderboardContainer == null || saveScript == null) return;
        
        // Töröljük a régi bejegyzéseket
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Leaderboard betöltése
        saveScript.LoadLeaderboard();
        var topScores = saveScript.GetTopScores(10);
        
        // Új bejegyzések létrehozása
        for (int i = 0; i < topScores.Count; i++)
        {
            SaveData entry = topScores[i];
            GameObject entryObj;
            
            if (leaderboardEntryPrefab != null)
            {
                entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }
            else
            {
                // Ha nincs prefab, hozzunk létre egy egyszerű text objektumot
                entryObj = new GameObject($"Entry_{i}");
                entryObj.transform.SetParent(leaderboardContainer);
                entryObj.AddComponent<TextMeshProUGUI>();
            }
            
            // Text beállítása
            TextMeshProUGUI text = entryObj.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (text != null)
            {
                int minutes = entry.time / 60;
                int seconds = entry.time % 60;
                text.text = $"{i + 1}. {entry.playerName} - Score: {entry.highScore} - Time: {minutes:00}:{seconds:00}";
            }
        }
        
        Debug.Log($"Leaderboard refreshed with {topScores.Count} entries");
    }

    public void ClearLeaderboard()
    {
        if (saveScript != null)
        {
            saveScript.ClearLeaderboard();
            RefreshLeaderboard();
        }
    }
}
