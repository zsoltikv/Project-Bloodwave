using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterSortingByBand : MonoBehaviour
{
    public Transform feetPoint;
    public SpriteRenderer shadowRenderer;

    SpriteRenderer sr;
    int currentOrder;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (feetPoint == null || SortingBandManager.Instance == null)
            return;

        int newOrder =
            SortingBandManager.Instance.GetSortingOrder(feetPoint.position.y);

        if (newOrder != currentOrder)
        {
            currentOrder = newOrder;
            sr.sortingOrder = currentOrder;

            if(shadowRenderer != null) shadowRenderer.sortingOrder = currentOrder;
        }
    }
}
