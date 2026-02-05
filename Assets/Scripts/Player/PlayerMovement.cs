using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float deadZone = 15f;
    public float maxDragDistance = 120f;

    [Header("UI Indicator")]
    public RectTransform indicatorRoot;
    public RectTransform innerDot;

    [Header("Canvas")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Camera uiCamera;

    [Header("Shadow")]
    [SerializeField] private Transform shadow;

    public bool isAlive = true;

    private Vector3 shadowBaseScale;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AimDirection2D aim;

    private Vector2 startPos;
    private Vector2 dragVector;
    private bool dragging;

    private float radius;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aim = GetComponent<AimDirection2D>();

        indicatorRoot.gameObject.SetActive(false);

        radius = indicatorRoot.sizeDelta.x * 0.5f;

        shadowBaseScale = shadow.localScale;

        if (uiCamera == null && uiCanvas != null)
            uiCamera = uiCanvas.worldCamera;
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    private void FixedUpdate()
    {
        Move();
        animator.SetFloat("linearVelocity", rb.linearVelocity.magnitude);
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 0)
        {
            EndDrag();
            return;
        }

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
            BeginDrag(t.position);
        else if (t.phase == TouchPhase.Moved && dragging)
            UpdateDrag(t.position);
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            EndDrag();
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0) && dragging)
            UpdateDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    private void BeginDrag(Vector2 screenPos)
    {
        if (!isAlive) return;

        dragging = true;

        RectTransform canvasRect = uiCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCamera,
            out Vector2 localPoint
        );

        startPos = localPoint;

        indicatorRoot.anchoredPosition = localPoint;
        innerDot.anchoredPosition = Vector2.zero;
        indicatorRoot.gameObject.SetActive(true);
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        if (!isAlive) return;

        RectTransform canvasRect = uiCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCamera,
            out Vector2 localPoint
        );

        dragVector = localPoint - startPos;

        Vector2 clamped = Vector2.ClampMagnitude(dragVector, radius);
        innerDot.anchoredPosition = clamped;
    }

    public void EndDrag()
    {
        dragging = false;
        dragVector = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        if (indicatorRoot != null)
            indicatorRoot.gameObject.SetActive(false);
    }

    private void Move()
    {
        if (!isAlive || !dragging || dragVector.magnitude < deadZone)
        {
            rb.linearVelocity = Vector2.zero;

            shadow.localScale = Vector3.Lerp(
                shadow.localScale,
                shadowBaseScale,
                Time.deltaTime * 10f
            );

            return;
        }

        float strength = Mathf.Clamp(dragVector.magnitude / maxDragDistance, 0f, 1f);
        Vector2 direction = dragVector.normalized;

        rb.linearVelocity = direction * maxSpeed * strength;
        aim.SetFromMove(direction);

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        float t = rb.linearVelocity.magnitude / maxSpeed;

        Vector3 targetScale = new Vector3(
            shadowBaseScale.x * Mathf.Lerp(1f, 1.1f, t),
            shadowBaseScale.y,
            shadowBaseScale.z
        );

        shadow.localScale = Vector3.Lerp(
            shadow.localScale,
            targetScale,
            Time.deltaTime * 10f
        );
    }
}