using TMPro;
using UnityEngine;

public class DamageTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    [Header("Anim")]
    [SerializeField] private float lifetime = 0.75f;
    [SerializeField] private float rise = 60f;     // canvas unit (kb pixel)
    [SerializeField] private float drift = 25f;
    [SerializeField] private float arc = 15f;

    private RectTransform rt;
    private float t;
    private Vector2 start;
    private Vector2 end;
    private Color baseColor;

    private void Awake()
    {
        rt = (RectTransform)transform;
        if (text == null) text = GetComponent<TMP_Text>();
    }

    // Spawner hívja még Init elõtt
    public void SetStartAnchoredPosition(Vector2 anchoredPos)
    {
        start = anchoredPos;
        rt.anchoredPosition = start;
    }

    public void Init(float damage)
    {
        string dmgStr = (Mathf.Abs(damage - Mathf.Round(damage)) < 0.01f)
            ? Mathf.RoundToInt(damage).ToString()
            : damage.ToString("0.0");

        text.text = dmgStr;

        float dir = Random.value < 0.5f ? -1f : 1f;
        Vector2 offset = new Vector2(drift * dir + Random.Range(-8f, 8f),
                                     rise + Random.Range(-6f, 6f));
        end = start + offset;

        t = 0f;
    }

    private void Update()
    {
        t += Time.deltaTime / lifetime;
        float u = Mathf.Clamp01(t);

        float eased = u * u * (3f - 2f * u); // SmoothStep

        Vector2 pos = Vector2.Lerp(start, end, eased);
        pos.y += Mathf.Sin(u * Mathf.PI) * arc;

        rt.anchoredPosition = pos;

        Color c = text.color;
        c.a = Mathf.Lerp(1f, 0f, eased);
        text.color = c;

        if (u >= 1f) Destroy(gameObject);
    }
}
