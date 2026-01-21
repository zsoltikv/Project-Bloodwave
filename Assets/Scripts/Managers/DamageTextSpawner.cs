using TMPro;
using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera uiCamera;

    [Header("UI Parent")]
    [SerializeField] private RectTransform parent;          // DamageTextContainer

    [Header("Prefab")]
    [SerializeField] private DamageTextUI damageTextPrefab;

    private RectTransform canvasRect;

    private void Awake()
    {
        Instance = this;

        if (uiCanvas != null)
            canvasRect = uiCanvas.transform as RectTransform;

        // ugyanaz mint nálatok a PlayerMovement-ben:
        if (uiCamera == null && uiCanvas != null)
            uiCamera = uiCanvas.worldCamera;

        if (uiCamera == null)
            uiCamera = Camera.main;
    }

    public void Spawn(float dmg, Vector3 worldPos, string critType)
    {
        if (damageTextPrefab == null || parent == null || uiCanvas == null) return;

        Camera camForRect = (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : uiCamera;
        if (camForRect == null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay && uiCamera == null) return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(camForRect, worldPos);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform, screenPos, camForRect, out var localPoint))
            return;

        var dt = Instantiate(damageTextPrefab, parent);
        dt.SetStartAnchoredPosition(localPoint);

        Color32 color = new Color32(255, 255, 255, 255);
        switch ((critType ?? "").ToLowerInvariant())
        {
            case "normal": color = new Color32(255, 255, 0, 255); break;
            case "extra": color = new Color32(255, 155, 0, 255); break;
            case "bleed": color = new Color32(255, 0, 0, 255); break;
        }

        var tmp = dt.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.color = color;

        dt.Init(dmg);
    }

}
