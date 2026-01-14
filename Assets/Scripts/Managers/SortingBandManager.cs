using System.Collections.Generic;
using UnityEngine;

public class SortingBandManager : MonoBehaviour
{
    public static SortingBandManager Instance;

    [Header("Band Settings")]
    public float bandHeight = 0.1f;
    public int minOrder = 0;
    public int maxOrder = 300;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetSortingOrder(float worldY)
    {
        int order = -Mathf.RoundToInt(worldY / bandHeight);
        return Mathf.Clamp(order, minOrder, maxOrder);
    }
}