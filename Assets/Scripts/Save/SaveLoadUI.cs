using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveListPanel;
    public GameObject saveItemPrefab;
    public Transform saveListContainer;
    
    [Header("Save Input")]
    public TMP_InputField nameInputField;
    public Button saveButton;
    
    [Header("Managers")]
    public SaveScript saveScript;

    void Start()
    {
        if (saveScript == null)
        {
            saveScript = FindAnyObjectByType<SaveScript>();
        }
        
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClick);
        }
        
        RefreshSaveList();
    }

    public void OnSaveButtonClick()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            saveScript.SetPlayerName(nameInputField.text);
        }
        
        saveScript.SaveGame();
        RefreshSaveList();
    }

    public void RefreshSaveList()
    {
        if (saveListContainer == null || saveItemPrefab == null) return;
        
        // Clear existing items
        foreach (Transform child in saveListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new items
        for (int i = 0; i < saveScript.saveDataList.Count; i++)
        {
            SaveData save = saveScript.saveDataList[i];
            GameObject item = Instantiate(saveItemPrefab, saveListContainer);
            
            // Set text (customize based on your prefab structure)
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                texts[0].text = $"{save.playerName} - Lvl {save.highScore} - Score: {save.highScore}";
            }
        }
    }


    public void ShowSaveList()
    {
        if (saveListPanel != null)
        {
            saveListPanel.SetActive(true);
            RefreshSaveList();
        }
    }

    public void HideSaveList()
    {
        if (saveListPanel != null)
        {
            saveListPanel.SetActive(false);
        }
    }
}
