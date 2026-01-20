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

    public void Spawn(float dmg, Vector3 worldPos)
    {
        if (damageTextPrefab == null || parent == null || uiCanvas == null || canvasRect == null) return;
        if (uiCamera == null) return;

        // 1) world -> screen
        Vector2 screenPos = uiCamera.WorldToScreenPoint(worldPos);

        // 2) screen -> canvas local
        Vector2 localPoint;
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCamera,
            out localPoint
        );

        if (!ok) return;

        // 3) spawn és anchoredPosition beállítás
        var dt = Instantiate(damageTextPrefab, parent);
        dt.SetStartAnchoredPosition(localPoint);
        dt.Init(dmg);
    }
}
