using UnityEngine;
using UnityEngine.UI; // Szükséges a Button és InputField eléréséhez
using TMPro;           // Ha TextMeshPro-t használsz (ajánlott)

public class DebugPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TMP_InputField sizeInput;
    [SerializeField] private Button exitButton;

    [Header("Settings")]
    [SerializeField] private Camera targetCamera;

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        exitButton.onClick.AddListener(HidePanel);
        sizeInput.onValueChanged.AddListener(delegate { OnInputChanged(); });

        UpdateInputText();
    }

    public void ShowPanel()
    {
        debugPanel.SetActive(true);
        GameManagerScript.instance.PauseGame();
        UpdateInputText();
    }

    public void HidePanel()
    {
        debugPanel.SetActive(false);
        GameManagerScript.instance.ResumeGame();
    }

    private void UpdateInputText()
    {
        if (targetCamera != null && sizeInput != null)
        {
            sizeInput.text = targetCamera.orthographicSize.ToString();
        }
    }

    public void OnInputChanged()
    {
        if (float.TryParse(sizeInput.text, out float newValue))
        {
            targetCamera.orthographicSize = newValue;
        }
    }
}