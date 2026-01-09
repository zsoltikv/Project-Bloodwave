using UnityEngine;
using UnityEngine.U2D;
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
    public float indicatorRadius = 60f;

    [Header("Shadow")]
    [SerializeField] private Transform shadow;

    private Vector3 shadowBaseScale;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 startPos;
    private Vector2 dragVector;
    private bool dragging;
    private AimDirection2D aim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        indicatorRoot.gameObject.SetActive(false);
        aim = GetComponent<AimDirection2D>();

        shadowBaseScale = shadow.localScale;
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void FixedUpdate()
    {
        Move();
        animator.SetFloat("linearVelocity", rb.linearVelocity.magnitude);
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0)
        {
            EndDrag();
            return;
        }

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            BeginDrag(t.position);
        }
        else if (t.phase == TouchPhase.Moved && dragging)
        {
            UpdateDrag(t.position);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            EndDrag();
        }
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0) && dragging)
            UpdateDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            EndDrag();
    }

    void BeginDrag(Vector2 screenPos)
    {
        startPos = screenPos;
        dragging = true;

        indicatorRoot.position = screenPos;
        indicatorRoot.gameObject.SetActive(true);
        innerDot.anchoredPosition = Vector2.zero;
    }

    void UpdateDrag(Vector2 screenPos)
    {
        dragVector = screenPos - startPos;

        Vector2 clamped = Vector2.ClampMagnitude(dragVector, indicatorRadius);
        innerDot.anchoredPosition = clamped;
    }

    void EndDrag()
    {
        dragging = false;
        dragVector = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        indicatorRoot.gameObject.SetActive(false);
    }

    void Move()
    {
        if (!dragging || dragVector.magnitude < deadZone)
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
